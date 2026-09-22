using Microsoft.EntityFrameworkCore;
using dashboard.Components.Account;
using dashboard.Models.Reclutamiento;

namespace dashboard.Data;

// Portal público de postulación (sin cuenta de Identity) — mismo patrón que SolicitudService/
// PerfilCargoService: un DbContext propio por consulta. El candidato entra sin sesión; el "acceso"
// es un link mágico por correo (PostulanteAccesoToken), no un login real.
public class PostulacionPublicaService(IDbContextFactory<ApplicationDbContext> dbFactory, CorreoSistemaService correo, BlobStorageService blobStorage)
{
    private static readonly TimeSpan VigenciaToken = TimeSpan.FromHours(48);

    public record SolicitudPublicaResumen(int Id, string CargoNombre, string ClienteRazonSocial, DateTime FechaInicioServicio, string? DireccionServicio);

    public record SolicitudPublicaDetalle(int Id, string CargoNombre, string ClienteRazonSocial, string? Descripcion, string? Requisitos, string? ObjetivoCargo, string? EducacionMinima, int? AniosExperienciaMinimo, string? ConocimientosTecnicos, string? Habilidades, string? CondicionesEspeciales, DateTime FechaInicioServicio, string? DireccionServicio);

    public record TokenInfo(string Token, int SolicitudId, string CargoNombre, string NombreContacto, string CorreoContacto, string TelefonoContacto, bool Valido);

    public record PostulanteDatosInput(string RUT, string? Sexo, int? Edad, string? Comuna, int? AniosExperiencia, string? RubroExperiencia, string? EstadoCivil, string? Nacionalidad, bool? Discapacidad, decimal? RentaPretendida, string? DescripcionProfesional, string? Habilidades);

    public record DocumentoInput(string Tipo, string NombreArchivo, Stream Contenido);

    public async Task<List<SolicitudPublicaResumen>> ListarSolicitudesAbiertasAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        var query = from s in db.Solicitudes
                     where s.Estado == "Activa"
                     join c in db.Clientes on s.ClienteId equals c.Id
                     join pv in db.PerfilCargoVersiones on s.PerfilCargoVersionId equals pv.Id
                     join pc in db.PerfilesCargo on pv.PerfilCargoId equals pc.Id
                     select new { s.Id, CargoNombre = pc.Nombre, c.RazonSocial, s.FechaInicioServicio, s.DireccionServicio };

        var resultado = await query.OrderBy(x => x.FechaInicioServicio).ToListAsync();
        return resultado.Select(x => new SolicitudPublicaResumen(x.Id, x.CargoNombre, x.RazonSocial, x.FechaInicioServicio, x.DireccionServicio)).ToList();
    }

    public async Task<SolicitudPublicaDetalle?> ObtenerSolicitudPublicaAsync(int solicitudId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        var query = from s in db.Solicitudes
                     where s.Id == solicitudId && s.Estado == "Activa"
                     join c in db.Clientes on s.ClienteId equals c.Id
                     join pv in db.PerfilCargoVersiones on s.PerfilCargoVersionId equals pv.Id
                     join pc in db.PerfilesCargo on pv.PerfilCargoId equals pc.Id
                     select new SolicitudPublicaDetalle(s.Id, pc.Nombre, c.RazonSocial, pc.Descripcion, pc.Requisitos, pv.ObjetivoCargo, pv.EducacionMinima, pv.AniosExperienciaMinimo, pv.ConocimientosTecnicos, pv.Habilidades, pv.CondicionesEspeciales, s.FechaInicioServicio, s.DireccionServicio);

        return await query.SingleOrDefaultAsync();
    }

    // Un único correo cubre "confirmar el correo" + "aceptar la política de datos personales" — más
    // simple que el flujo en dos correos separados del enunciado original (Modulos.txt).
    public async Task IniciarPostulacionAsync(int solicitudId, string nombre, string correoDestino, string telefono, string urlBase)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        var solicitud = await db.Solicitudes.SingleOrDefaultAsync(s => s.Id == solicitudId && s.Estado == "Activa")
            ?? throw new InvalidOperationException("La Solicitud no existe o ya no está activa.");

        var token = new PostulanteAccesoToken
        {
            Token = Guid.NewGuid().ToString("N"),
            SolicitudId = solicitudId,
            NombreContacto = nombre,
            CorreoContacto = correoDestino,
            TelefonoContacto = telefono,
            Proposito = "VerificacionInicial",
            FechaExpiracion = DateTime.UtcNow.Add(VigenciaToken),
        };
        db.PostulanteAccesoTokens.Add(token);
        await db.SaveChangesAsync();

        var link = $"{urlBase.TrimEnd('/')}/postulaciones/verificar/{token.Token}";
        await correo.EnviarCorreoGenericoAsync(correoDestino, "Confirma tu postulación - SOS Group",
            $"<p>Hola {nombre},</p><p>Para confirmar tu postulación y aceptar la política de datos personales, haz <a href='{link}'>clic aquí</a>.</p><p>Este link vence en 48 horas.</p>");
    }

    public async Task<TokenInfo?> ObtenerInfoTokenAsync(string token)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        var query = from t in db.PostulanteAccesoTokens
                     where t.Token == token
                     join s in db.Solicitudes on t.SolicitudId equals s.Id
                     join pv in db.PerfilCargoVersiones on s.PerfilCargoVersionId equals pv.Id
                     join pc in db.PerfilesCargo on pv.PerfilCargoId equals pc.Id
                     select new { t, CargoNombre = pc.Nombre };

        var fila = await query.SingleOrDefaultAsync();
        if (fila is null) return null;

        var valido = fila.t.UsadoEn is null && fila.t.FechaExpiracion > DateTime.UtcNow;
        return new TokenInfo(fila.t.Token, fila.t.SolicitudId, fila.CargoNombre, fila.t.NombreContacto, fila.t.CorreoContacto, fila.t.TelefonoContacto, valido);
    }

    // Recién acá se crea/actualiza el Postulante (el RUT no se conoce antes de este paso) y se marca
    // el token como usado — "aceptar la política" y "completar datos" son una sola transacción.
    public async Task<bool> CompletarDatosAsync(string token, PostulanteDatosInput datos, List<DocumentoInput> documentos)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        await using var tx = await db.Database.BeginTransactionAsync();

        var accesoToken = await db.PostulanteAccesoTokens.SingleOrDefaultAsync(t => t.Token == token);
        if (accesoToken is null || accesoToken.UsadoEn is not null || accesoToken.FechaExpiracion <= DateTime.UtcNow)
        {
            return false;
        }

        var rut = NormalizarRut(datos.RUT);
        var postulante = await db.Postulantes.SingleOrDefaultAsync(p => p.RUT == rut);
        if (postulante is null)
        {
            postulante = new Postulante { RUT = rut, Nombre = accesoToken.NombreContacto, Correo = accesoToken.CorreoContacto, Telefono = accesoToken.TelefonoContacto };
            db.Postulantes.Add(postulante);
        }

        // Upsert sin pisar campos ya llenos con valores vacíos (mismo criterio documentado para la
        // carga masiva de Atracción en Reclutamiento2.md).
        postulante.Sexo ??= datos.Sexo;
        postulante.Edad ??= datos.Edad;
        postulante.Comuna ??= datos.Comuna;
        postulante.AniosExperiencia ??= datos.AniosExperiencia;
        postulante.RubroExperiencia ??= datos.RubroExperiencia;
        postulante.EstadoCivil ??= datos.EstadoCivil;
        postulante.Nacionalidad ??= datos.Nacionalidad;
        postulante.Discapacidad ??= datos.Discapacidad;
        postulante.RentaPretendida ??= datos.RentaPretendida;
        postulante.DescripcionProfesional ??= datos.DescripcionProfesional;
        postulante.Habilidades ??= datos.Habilidades;
        postulante.PoliticaAceptada = true;
        postulante.FechaAceptacionPolitica = DateTime.UtcNow;
        await db.SaveChangesAsync();

        foreach (var documento in documentos)
        {
            var rutaBlob = await blobStorage.SubirDocumentoPostulanteAsync(postulante.Id, documento.NombreArchivo, documento.Contenido);
            db.PostulanteDocumentos.Add(new PostulanteDocumento { PostulanteId = postulante.Id, Tipo = documento.Tipo, RutaArchivo = rutaBlob, FechaCarga = DateTime.UtcNow });
        }

        var yaPostulado = await db.PostulanteSolicitudes.AnyAsync(ps => ps.PostulanteId == postulante.Id && ps.SolicitudId == accesoToken.SolicitudId);
        if (!yaPostulado)
        {
            db.PostulanteSolicitudes.Add(new PostulanteSolicitud { PostulanteId = postulante.Id, SolicitudId = accesoToken.SolicitudId, Origen = "Directa", FechaIngreso = DateTime.UtcNow });
        }

        accesoToken.PostulanteId = postulante.Id;
        accesoToken.UsadoEn = DateTime.UtcNow;
        await db.SaveChangesAsync();

        await tx.CommitAsync();
        return true;
    }

    private static string NormalizarRut(string rut) => rut.Trim().Replace(".", "").Replace(" ", "").ToUpperInvariant();
}
