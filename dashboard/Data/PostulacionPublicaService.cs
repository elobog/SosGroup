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
    private static readonly TimeSpan VigenciaTokenAccesoRetorno = TimeSpan.FromMinutes(10);

    public record SolicitudPublicaResumen(int Id, string CargoNombre, string ClienteRazonSocial, DateTime FechaInicioServicio, string? DireccionServicio);

    public record SolicitudPublicaDetalle(int Id, string CargoNombre, string ClienteRazonSocial, string? Descripcion, string? Requisitos, string? ObjetivoCargo, string? EducacionMinima, int? AniosExperienciaMinimo, string? ConocimientosTecnicos, string? Habilidades, string? CondicionesEspeciales, DateTime FechaInicioServicio, string? DireccionServicio);

    public record TokenInfo(string Token, int SolicitudId, string CargoNombre, string NombreContacto, string CorreoContacto, string TelefonoContacto, bool Valido, string? RutConocido);

    public record PostulanteDatosInput(string RUT, string? Sexo, int? Edad, string? Comuna, int? AniosExperiencia, string? RubroExperiencia, string? EstadoCivil, string? Nacionalidad, bool? Discapacidad, decimal? RentaPretendida, string? DescripcionProfesional, string? Habilidades);

    public record DocumentoInput(string Tipo, string NombreArchivo, Stream Contenido);

    // Los 4 pasos de la línea de tiempo pública: recibida siempre true, DatosCompletos vive en el
    // Postulante (política aceptada — no todos la tienen aún, ej. carga masiva sin invitar todavía),
    // SeleccionadoPreseleccion y EtapaEvaluacion son los mismos campos que ya usa Reclutamiento.
    public record MiPostulacionResumen(int SolicitudId, string CargoNombre, string ClienteRazonSocial, DateTime FechaIngreso, bool SeleccionadoPreseleccion, string? EtapaEvaluacion);

    public record MiCuentaInfo(string Token, string Nombre, string RUT, string? Sexo, int? Edad, string? Comuna, int? AniosExperiencia, string? RubroExperiencia, bool DatosCompletos, List<MiPostulacionResumen> Postulaciones, List<string> DocumentosCargados);

    public record MiCuentaDatosInput(string? Sexo, int? Edad, string? Comuna, int? AniosExperiencia, string? RubroExperiencia);

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

        // Si correo y teléfono coinciden con un postulante que ya completó su registro antes (RUT y
        // documentos ya están), no tiene sentido hacerlo pasar de nuevo por "verificar + completar
        // datos" — se le registra la postulación directo y se le manda el acceso a Mi Cuenta.
        var postulanteExistente = await db.Postulantes
            .SingleOrDefaultAsync(p => p.Correo == correoDestino && p.Telefono == telefono && p.PoliticaAceptada);
        if (postulanteExistente is not null)
        {
            var yaPostulado = await db.PostulanteSolicitudes
                .AnyAsync(ps => ps.PostulanteId == postulanteExistente.Id && ps.SolicitudId == solicitudId);
            if (!yaPostulado)
            {
                db.PostulanteSolicitudes.Add(new PostulanteSolicitud { PostulanteId = postulanteExistente.Id, SolicitudId = solicitudId, Origen = "Directa", FechaIngreso = DateTime.UtcNow });
                await db.SaveChangesAsync();
            }

            await EnviarLinkAccesoRetornoAsync(db, postulanteExistente, urlBase);
            return;
        }

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

    // Invita a completar el portal público a un Postulante que ya existe (llegó por carga masiva de
    // CVs en Atracción). A diferencia de IniciarPostulacionAsync, el token ya viene con PostulanteId
    // seteado — CompletarDatosAsync lo usa para no crear un Postulante duplicado. Reutiliza las
    // mismas páginas públicas (/postulaciones/verificar y /postulaciones/datos), sin páginas nuevas.
    public async Task InvitarAsync(int postulanteSolicitudId, string urlBase)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        var datos = await (from ps in db.PostulanteSolicitudes
                            where ps.Id == postulanteSolicitudId
                            join p in db.Postulantes on ps.PostulanteId equals p.Id
                            select new { ps.SolicitudId, Postulante = p })
            .SingleOrDefaultAsync();
        if (datos is null) throw new InvalidOperationException("No se encontró la postulación.");
        if (string.IsNullOrWhiteSpace(datos.Postulante.Correo))
        {
            throw new InvalidOperationException("El postulante no tiene correo registrado — no se puede invitar.");
        }

        var token = new PostulanteAccesoToken
        {
            Token = Guid.NewGuid().ToString("N"),
            SolicitudId = datos.SolicitudId,
            PostulanteId = datos.Postulante.Id,
            NombreContacto = datos.Postulante.Nombre,
            CorreoContacto = datos.Postulante.Correo,
            TelefonoContacto = datos.Postulante.Telefono ?? "",
            Proposito = "Invitacion",
            FechaExpiracion = DateTime.UtcNow.Add(VigenciaToken),
        };
        db.PostulanteAccesoTokens.Add(token);
        await db.SaveChangesAsync();

        var link = $"{urlBase.TrimEnd('/')}/postulaciones/verificar/{token.Token}";
        await correo.EnviarCorreoGenericoAsync(datos.Postulante.Correo, "Te invitamos a completar tu postulación - SOS Group",
            $"<p>Hola {datos.Postulante.Nombre},</p><p>Para completar tu postulación y aceptar la política de datos personales, haz <a href='{link}'>clic aquí</a>.</p><p>Este link vence en 48 horas.</p>");
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

        var rutConocido = fila.t.PostulanteId is not null
            ? (await db.Postulantes.FindAsync(fila.t.PostulanteId.Value))?.RUT
            : null;

        var valido = fila.t.UsadoEn is null && fila.t.FechaExpiracion > DateTime.UtcNow;
        return new TokenInfo(fila.t.Token, fila.t.SolicitudId!.Value, fila.CargoNombre, fila.t.NombreContacto, fila.t.CorreoContacto, fila.t.TelefonoContacto, valido, rutConocido);
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
        // Si el token ya viene con PostulanteId (invitación desde carga masiva), se resuelve por ahí
        // y no por el RUT tipeado — evita crear un duplicado si el candidato lo escribe distinto.
        var postulante = accesoToken.PostulanteId is not null
            ? await db.Postulantes.FindAsync(accesoToken.PostulanteId.Value)
            : await db.Postulantes.SingleOrDefaultAsync(p => p.RUT == rut);
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
            db.PostulanteSolicitudes.Add(new PostulanteSolicitud { PostulanteId = postulante.Id, SolicitudId = accesoToken.SolicitudId!.Value, Origen = "Directa", FechaIngreso = DateTime.UtcNow });
        }

        accesoToken.PostulanteId = postulante.Id;
        accesoToken.UsadoEn = DateTime.UtcNow;
        await db.SaveChangesAsync();

        await tx.CommitAsync();
        return true;
    }

    // "Ya postulé, quiero editar mis datos" — el candidato pide un link a su correo, no ingresa clave.
    // Por seguridad la respuesta es siempre la misma exista o no el correo (no revela si está registrado).
    public async Task SolicitarAccesoRetornoAsync(string correoDestino, string urlBase)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        var postulante = await db.Postulantes.SingleOrDefaultAsync(p => p.Correo == correoDestino);
        if (postulante is null) return;

        await EnviarLinkAccesoRetornoAsync(db, postulante, urlBase);
    }

    private async Task EnviarLinkAccesoRetornoAsync(ApplicationDbContext db, Postulante postulante, string urlBase)
    {
        var token = new PostulanteAccesoToken
        {
            Token = Guid.NewGuid().ToString("N"),
            SolicitudId = null,
            PostulanteId = postulante.Id,
            NombreContacto = postulante.Nombre,
            CorreoContacto = postulante.Correo ?? "",
            TelefonoContacto = postulante.Telefono ?? "",
            Proposito = "AccesoRetorno",
            FechaExpiracion = DateTime.UtcNow.Add(VigenciaTokenAccesoRetorno),
        };
        db.PostulanteAccesoTokens.Add(token);
        await db.SaveChangesAsync();

        var link = $"{urlBase.TrimEnd('/')}/postulaciones/mi-cuenta/{token.Token}";
        await correo.EnviarCorreoGenericoAsync(postulante.Correo!, "Accede a tu postulación - SOS Group",
            $"<p>Hola {postulante.Nombre},</p><p>Haz <a href='{link}'>clic aquí</a> para acceder a tu postulación y revisar o actualizar tus datos.</p><p>Por seguridad, este link vence en 10 minutos.</p>");
    }

    // El token de AccesoRetorno actúa como una sesión corta (no se marca "usado" al primer clic) —
    // el candidato puede navegar y guardar cambios varias veces mientras no venza.
    public async Task<MiCuentaInfo?> ObtenerCuentaAsync(string token)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        var accesoToken = await db.PostulanteAccesoTokens.SingleOrDefaultAsync(t => t.Token == token && t.Proposito == "AccesoRetorno");
        if (accesoToken is null || accesoToken.PostulanteId is null || accesoToken.FechaExpiracion <= DateTime.UtcNow) return null;

        var postulante = await db.Postulantes.FindAsync(accesoToken.PostulanteId.Value);
        if (postulante is null) return null;

        var postulaciones = await (from ps in db.PostulanteSolicitudes
                                    where ps.PostulanteId == postulante.Id
                                    join s in db.Solicitudes on ps.SolicitudId equals s.Id
                                    join pv in db.PerfilCargoVersiones on s.PerfilCargoVersionId equals pv.Id
                                    join pc in db.PerfilesCargo on pv.PerfilCargoId equals pc.Id
                                    join c in db.Clientes on s.ClienteId equals c.Id
                                    orderby ps.FechaIngreso descending
                                    select new MiPostulacionResumen(s.Id, pc.Nombre, c.RazonSocial, ps.FechaIngreso, ps.SeleccionadoPreseleccion, ps.EtapaEvaluacion))
            .ToListAsync();

        var documentos = await db.PostulanteDocumentos
            .Where(d => d.PostulanteId == postulante.Id)
            .Select(d => d.Tipo)
            .Distinct()
            .ToListAsync();

        return new MiCuentaInfo(token, postulante.Nombre, postulante.RUT, postulante.Sexo, postulante.Edad, postulante.Comuna, postulante.AniosExperiencia, postulante.RubroExperiencia, postulante.PoliticaAceptada, postulaciones, documentos);
    }

    // A diferencia de CompletarDatosAsync (primera vez, no pisa datos existentes), acá el candidato está
    // editando lo suyo — lo que envía reemplaza el valor anterior. Los documentos nuevos se agregan sin
    // borrar los anteriores (mismo criterio que PerfilCargoDocumentoOriginal: se conserva el historial).
    public async Task<bool> ActualizarCuentaAsync(string token, MiCuentaDatosInput datos, List<DocumentoInput> documentos)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        var accesoToken = await db.PostulanteAccesoTokens.SingleOrDefaultAsync(t => t.Token == token && t.Proposito == "AccesoRetorno");
        if (accesoToken is null || accesoToken.PostulanteId is null || accesoToken.FechaExpiracion <= DateTime.UtcNow) return false;

        var postulante = await db.Postulantes.FindAsync(accesoToken.PostulanteId.Value);
        if (postulante is null) return false;

        postulante.Sexo = datos.Sexo;
        postulante.Edad = datos.Edad;
        postulante.Comuna = datos.Comuna;
        postulante.AniosExperiencia = datos.AniosExperiencia;
        postulante.RubroExperiencia = datos.RubroExperiencia;

        foreach (var documento in documentos)
        {
            var rutaBlob = await blobStorage.SubirDocumentoPostulanteAsync(postulante.Id, documento.NombreArchivo, documento.Contenido);
            db.PostulanteDocumentos.Add(new PostulanteDocumento { PostulanteId = postulante.Id, Tipo = documento.Tipo, RutaArchivo = rutaBlob, FechaCarga = DateTime.UtcNow });
        }

        await db.SaveChangesAsync();
        return true;
    }

    private static string NormalizarRut(string rut) => rut.Trim().Replace(".", "").Replace(" ", "").ToUpperInvariant();
}
