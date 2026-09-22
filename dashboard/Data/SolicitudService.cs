using Microsoft.EntityFrameworkCore;
using dashboard.Models;
using dashboard.Models.Reclutamiento;

namespace dashboard.Data;

// Mismo patrón que ClientesService/AccesoAppService: un DbContext propio por consulta.
public class SolicitudService(IDbContextFactory<ApplicationDbContext> dbFactory, BlobStorageService blobStorage)
{
    // Roles cuya visibilidad se acota a los clientes asignados (ClienteReclutador/ClienteSupervisor).
    // SuperAdmin/Admin/Supervisor Administrativo ven todo, sin filtro (BD_Dashboard.md § Identity — Acceso).
    private static readonly string[] RolesAcotados = ["Reclutador", "Supervisor Operaciones"];

    public record SolicitudResumen(int Id, string CodigoSolicitud, string ClienteRazonSocial, string PerfilCargoNombre, DateTime FechaInicioServicio, string SupervisorNombre, string Estado, int PostulantesCount);

    public async Task<List<SolicitudResumen>> ListarAsync(string usuarioId, IReadOnlyCollection<string> roles)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        var query = from s in db.Solicitudes
                     join c in db.Clientes on s.ClienteId equals c.Id
                     join pv in db.PerfilCargoVersiones on s.PerfilCargoVersionId equals pv.Id
                     join pc in db.PerfilesCargo on pv.PerfilCargoId equals pc.Id
                     join sup in db.Users on s.SupervisorId equals sup.Id
                     select new { s, c.RazonSocial, PerfilNombre = pc.Nombre, SupervisorNombre = sup.Nombre };

        if (roles.Any(RolesAcotados.Contains) && !roles.Any(r => r is "SuperAdmin" or "Admin" or "Supervisor Administrativo"))
        {
            var clienteIds = await ClientesAsignadosAsync(db, usuarioId, roles);
            query = query.Where(x => clienteIds.Contains(x.s.ClienteId));
        }

        var resultado = await query.OrderByDescending(x => x.s.FechaCreacion).ToListAsync();
        var solicitudIds = resultado.Select(x => x.s.Id).ToList();
        var conteos = await db.PostulanteSolicitudes
            .Where(ps => solicitudIds.Contains(ps.SolicitudId))
            .GroupBy(ps => ps.SolicitudId)
            .Select(g => new { SolicitudId = g.Key, Cantidad = g.Count() })
            .ToDictionaryAsync(x => x.SolicitudId, x => x.Cantidad);

        return resultado.Select(x => new SolicitudResumen(x.s.Id, x.s.CodigoSolicitud, x.RazonSocial, x.PerfilNombre, x.s.FechaInicioServicio, x.SupervisorNombre, x.s.Estado, conteos.GetValueOrDefault(x.s.Id))).ToList();
    }

    private static async Task<HashSet<int>> ClientesAsignadosAsync(ApplicationDbContext db, string usuarioId, IReadOnlyCollection<string> roles)
    {
        var clienteIds = new HashSet<int>();
        if (roles.Contains("Reclutador"))
        {
            clienteIds.UnionWith(await db.ClienteReclutadores.Where(cr => cr.UsuarioId == usuarioId).Select(cr => cr.ClienteId).ToListAsync());
        }
        if (roles.Contains("Supervisor Operaciones"))
        {
            clienteIds.UnionWith(await db.ClienteSupervisores.Where(cs => cs.UsuarioId == usuarioId).Select(cs => cs.ClienteId).ToListAsync());
        }
        return clienteIds;
    }

    public async Task<List<int>> ListarClientesAsignadosAsync(string usuarioId, IReadOnlyCollection<string> roles)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return (await ClientesAsignadosAsync(db, usuarioId, roles)).ToList();
    }

    public async Task<Solicitud?> ObtenerAsync(int id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Solicitudes.FindAsync(id);
    }

    public async Task<List<Solicitud>> ListarPorClienteAsync(int clienteId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Solicitudes.Where(s => s.ClienteId == clienteId).OrderByDescending(s => s.FechaCreacion).ToListAsync();
    }

    public async Task<List<SolicitudDetalle>> ListarRondasAsync(int solicitudId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.SolicitudDetalles.Where(d => d.SolicitudId == solicitudId).OrderBy(d => d.FechaSolicitud).ToListAsync();
    }

    public record PostulanteDocumentoResumen(int Id, string Tipo, DateTime FechaCarga);
    public record PostulanteRecibido(
        int PostulanteSolicitudId, int PostulanteId, string Nombre, string RUT, string? Correo, string? Telefono,
        string? Sexo, int? Edad, string? Comuna, int? AniosExperiencia, string? RubroExperiencia,
        string Origen, int? Puntaje, bool Disponible,
        DateTime FechaIngreso, bool SeleccionadoPreseleccion, List<PostulanteDocumentoResumen> Documentos);

    public static string EtiquetaDocumento(string tipo) => tipo switch
    {
        "CV" => "Currículum",
        "CertificadoAntecedentes" => "Cert. Antecedentes",
        "CertificadoIsapreFonasa" => "Cert. Isapre/Fonasa",
        "CertificadoAFP" => "Cert. AFP",
        "UltimoFiniquito" => "Último Finiquito",
        _ => tipo,
    };

    // Módulo Postulación (Fase 1) — lectura de los postulantes que entraron por el portal público
    // para esta Solicitud. La usan tanto el resumen de la ficha como la tabla completa de Atracción.
    public async Task<List<PostulanteRecibido>> ListarPostulantesAsync(int solicitudId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        var postulantes = await (from ps in db.PostulanteSolicitudes
                                  where ps.SolicitudId == solicitudId
                                  join p in db.Postulantes on ps.PostulanteId equals p.Id
                                  orderby ps.FechaIngreso descending
                                  select new
                                  {
                                      PostulanteSolicitudId = ps.Id,
                                      PostulanteId = p.Id,
                                      p.Nombre,
                                      p.RUT,
                                      p.Correo,
                                      p.Telefono,
                                      p.Sexo,
                                      p.Edad,
                                      p.Comuna,
                                      p.AniosExperiencia,
                                      p.RubroExperiencia,
                                      ps.Origen,
                                      ps.Puntaje,
                                      Disponible = p.PoliticaAceptada,
                                      ps.FechaIngreso,
                                      ps.SeleccionadoPreseleccion,
                                  })
            .ToListAsync();

        var postulanteIds = postulantes.Select(p => p.PostulanteId).ToList();
        var documentos = await db.PostulanteDocumentos
            .Where(d => postulanteIds.Contains(d.PostulanteId))
            .OrderBy(d => d.FechaCarga)
            .ToListAsync();

        return postulantes.Select(p => new PostulanteRecibido(
            p.PostulanteSolicitudId, p.PostulanteId, p.Nombre, p.RUT, p.Correo, p.Telefono,
            p.Sexo, p.Edad, p.Comuna, p.AniosExperiencia, p.RubroExperiencia,
            p.Origen, p.Puntaje, p.Disponible,
            p.FechaIngreso, p.SeleccionadoPreseleccion,
            documentos.Where(d => d.PostulanteId == p.PostulanteId).Select(d => new PostulanteDocumentoResumen(d.Id, d.Tipo, d.FechaCarga)).ToList()
        )).ToList();
    }

    public record ActivarResultado(int Activados, int Omitidos);

    // Activa postulantes hacia Preselección (Módulo 2). Acotado a solicitudId para que no se pueda
    // activar de otra Solicitud. Gate (v6 de la maqueta): solo se activa a quien ya completó el
    // portal público (Postulante.PoliticaAceptada) — los de carga masiva sin invitar quedan omitidos.
    public async Task<ActivarResultado> ActivarPostulantesAsync(int solicitudId, List<int> postulanteSolicitudIds, string usuarioId)
    {
        if (postulanteSolicitudIds.Count == 0) return new ActivarResultado(0, 0);

        await using var db = await dbFactory.CreateDbContextAsync();

        var filas = await (from ps in db.PostulanteSolicitudes
                            where ps.SolicitudId == solicitudId && postulanteSolicitudIds.Contains(ps.Id) && !ps.SeleccionadoPreseleccion
                            join p in db.Postulantes on ps.PostulanteId equals p.Id
                            select new { ps, p.PoliticaAceptada })
            .ToListAsync();

        var activables = filas.Where(f => f.PoliticaAceptada).ToList();
        var omitidos = filas.Count - activables.Count;

        foreach (var f in activables)
        {
            f.ps.SeleccionadoPreseleccion = true;
            f.ps.EtapaPreseleccion = "Preseleccionado";
        }

        if (activables.Count > 0)
        {
            db.LogsActividad.Add(new LogActividad
            {
                UsuarioId = usuarioId,
                Accion = $"Activó {activables.Count} postulante(s) para Preselección",
                Entidad = "Solicitud",
                EntidadId = solicitudId,
            });
        }
        if (omitidos > 0)
        {
            db.LogsActividad.Add(new LogActividad
            {
                UsuarioId = usuarioId,
                Accion = $"{omitidos} postulante(s) omitido(s) — todavía no completan su Postulación (invitalos primero)",
                Entidad = "Solicitud",
                EntidadId = solicitudId,
            });
        }

        await db.SaveChangesAsync();
        return new ActivarResultado(activables.Count, omitidos);
    }

    public async Task<PrecalificacionConfig?> ObtenerConfigPrecalificacionAsync(int solicitudId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.PrecalificacionConfigs.SingleOrDefaultAsync(c => c.SolicitudId == solicitudId);
    }

    public async Task GuardarConfigPrecalificacionAsync(PrecalificacionConfig config)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var existente = await db.PrecalificacionConfigs.SingleOrDefaultAsync(c => c.SolicitudId == config.SolicitudId);
        if (existente is null)
        {
            db.PrecalificacionConfigs.Add(new PrecalificacionConfig
            {
                SolicitudId = config.SolicitudId,
                SexoPeso = config.SexoPeso,
                SexoObjetivo = config.SexoObjetivo,
                EdadPeso = config.EdadPeso,
                EdadMin = config.EdadMin,
                EdadMax = config.EdadMax,
                ComunaPeso = config.ComunaPeso,
                ComunaObjetivo = config.ComunaObjetivo,
                ExperienciaPeso = config.ExperienciaPeso,
                ExperienciaMinimo = config.ExperienciaMinimo,
                RubroPeso = config.RubroPeso,
                RubroObjetivo = config.RubroObjetivo,
            });
        }
        else
        {
            existente.SexoPeso = config.SexoPeso;
            existente.SexoObjetivo = config.SexoObjetivo;
            existente.EdadPeso = config.EdadPeso;
            existente.EdadMin = config.EdadMin;
            existente.EdadMax = config.EdadMax;
            existente.ComunaPeso = config.ComunaPeso;
            existente.ComunaObjetivo = config.ComunaObjetivo;
            existente.ExperienciaPeso = config.ExperienciaPeso;
            existente.ExperienciaMinimo = config.ExperienciaMinimo;
            existente.RubroPeso = config.RubroPeso;
            existente.RubroObjetivo = config.RubroObjetivo;
        }
        await db.SaveChangesAsync();
    }

    // Fórmula portada 1:1 desde scorePostulante() de la maqueta (Artifact) — ver el plan de esta sesión.
    internal static int CalcularPuntaje(Postulante p, PrecalificacionConfig cfg)
    {
        var sSexo = string.IsNullOrWhiteSpace(cfg.SexoObjetivo) ? 100 : (p.Sexo == cfg.SexoObjetivo ? 100 : 0);

        var edad = p.Edad ?? 0;
        int sEdad;
        if (edad >= cfg.EdadMin && edad <= cfg.EdadMax)
        {
            sEdad = 100;
        }
        else
        {
            var distancia = edad < cfg.EdadMin ? cfg.EdadMin - edad : edad - cfg.EdadMax;
            sEdad = Math.Max(0, 100 - Math.Min(distancia, 10) * 10);
        }

        var comunasObjetivo = (cfg.ComunaObjetivo ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => s.ToLowerInvariant())
            .ToList();
        var sComuna = comunasObjetivo.Count == 0 ? 100 : (comunasObjetivo.Contains((p.Comuna ?? "").ToLowerInvariant()) ? 100 : 0);

        var sExperiencia = cfg.ExperienciaMinimo <= 0
            ? 100
            : Math.Min(100, (int)Math.Round((p.AniosExperiencia ?? 0) / (double)cfg.ExperienciaMinimo * 100));

        var sRubro = string.IsNullOrWhiteSpace(cfg.RubroObjetivo)
            ? 100
            : ((p.RubroExperiencia ?? "").Contains(cfg.RubroObjetivo, StringComparison.OrdinalIgnoreCase) ? 100 : 0);

        var total = (sSexo * cfg.SexoPeso + sEdad * cfg.EdadPeso + sComuna * cfg.ComunaPeso + sExperiencia * cfg.ExperienciaPeso + sRubro * cfg.RubroPeso) / 100.0;
        return (int)Math.Round(total);
    }

    public async Task<int> CalcularPrecalificacionAsync(int solicitudId, string usuarioId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        var config = await db.PrecalificacionConfigs.SingleOrDefaultAsync(c => c.SolicitudId == solicitudId);
        if (config is null) return 0;

        var registros = await (from ps in db.PostulanteSolicitudes
                                where ps.SolicitudId == solicitudId
                                join p in db.Postulantes on ps.PostulanteId equals p.Id
                                select new { ps, p })
            .ToListAsync();

        foreach (var r in registros)
        {
            r.ps.Puntaje = CalcularPuntaje(r.p, config);
        }

        if (registros.Count > 0)
        {
            db.LogsActividad.Add(new LogActividad
            {
                UsuarioId = usuarioId,
                Accion = $"Calculó precalificación de {registros.Count} postulante(s)",
                Entidad = "Solicitud",
                EntidadId = solicitudId,
            });
        }

        await db.SaveChangesAsync();
        return registros.Count;
    }

    public record CandidatoCargaMasivaInput(string Nombre, string RUT, string Correo, string? Telefono, string? Sexo, int? Edad, string? Comuna, int? AniosExperiencia, string? RubroExperiencia, byte[] CvBytes, string CvNombreArchivo);

    // Carga masiva real de CVs (PDF, extracción previa por PostulanteIAService, revisada por el
    // Reclutador antes de llegar acá). Origen="CargaMasiva" — PoliticaAceptada queda en false hasta
    // que se invite y complete el portal público (ver PostulacionPublicaService.InvitarAsync).
    public async Task<int> GuardarCandidatosCargaMasivaAsync(int solicitudId, List<CandidatoCargaMasivaInput> candidatos, string usuarioId)
    {
        if (candidatos.Count == 0) return 0;

        await using var db = await dbFactory.CreateDbContextAsync();
        var guardados = 0;

        foreach (var c in candidatos)
        {
            var rut = NormalizarRut(c.RUT);
            if (string.IsNullOrWhiteSpace(rut)) continue;

            var postulante = await db.Postulantes.SingleOrDefaultAsync(p => p.RUT == rut);
            if (postulante is null)
            {
                postulante = new Postulante { RUT = rut, Nombre = c.Nombre };
                db.Postulantes.Add(postulante);
            }
            if (string.IsNullOrWhiteSpace(postulante.Nombre)) postulante.Nombre = c.Nombre;
            postulante.Correo ??= c.Correo;
            postulante.Telefono ??= c.Telefono;
            postulante.Sexo ??= c.Sexo;
            postulante.Edad ??= c.Edad;
            postulante.Comuna ??= c.Comuna;
            postulante.AniosExperiencia ??= c.AniosExperiencia;
            postulante.RubroExperiencia ??= c.RubroExperiencia;
            await db.SaveChangesAsync();

            var yaPostulado = await db.PostulanteSolicitudes.AnyAsync(ps => ps.PostulanteId == postulante.Id && ps.SolicitudId == solicitudId);
            if (!yaPostulado)
            {
                db.PostulanteSolicitudes.Add(new PostulanteSolicitud { PostulanteId = postulante.Id, SolicitudId = solicitudId, Origen = "CargaMasiva", FechaIngreso = DateTime.UtcNow });
            }

            var rutaBlob = await blobStorage.SubirDocumentoPostulanteAsync(postulante.Id, c.CvNombreArchivo, new MemoryStream(c.CvBytes));
            db.PostulanteDocumentos.Add(new PostulanteDocumento { PostulanteId = postulante.Id, Tipo = "CV", RutaArchivo = rutaBlob, FechaCarga = DateTime.UtcNow });

            guardados++;
        }

        if (guardados > 0)
        {
            db.LogsActividad.Add(new LogActividad
            {
                UsuarioId = usuarioId,
                Accion = $"Cargó {guardados} candidato(s) vía CV en PDF (IA)",
                Entidad = "Solicitud",
                EntidadId = solicitudId,
            });
        }

        await db.SaveChangesAsync();
        return guardados;
    }

    private static string NormalizarRut(string rut) => rut.Trim().Replace(".", "").Replace(" ", "").ToUpperInvariant();

    public async Task<int> CrearAsync(Solicitud solicitud, SolicitudDetalle primeraRonda)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        await using var tx = await db.Database.BeginTransactionAsync();

        var anio = DateTime.UtcNow.Year;
        var correlativo = await db.Solicitudes.CountAsync(s => s.FechaCreacion.Year == anio) + 1;
        solicitud.CodigoSolicitud = $"SOL-{anio}-{correlativo:0000}";
        solicitud.Estado = "Activa";
        db.Solicitudes.Add(solicitud);
        await db.SaveChangesAsync();

        primeraRonda.SolicitudId = solicitud.Id;
        primeraRonda.Estado = "Pendiente";
        db.SolicitudDetalles.Add(primeraRonda);
        await db.SaveChangesAsync();

        await tx.CommitAsync();
        return solicitud.Id;
    }

    public async Task ActualizarAsync(Solicitud solicitud)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.Solicitudes.Update(solicitud);
        await db.SaveChangesAsync();
    }

    public async Task AgregarRondaAsync(int solicitudId, SolicitudDetalle ronda)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        await using var tx = await db.Database.BeginTransactionAsync();

        ronda.SolicitudId = solicitudId;
        ronda.Estado = "Pendiente";
        db.SolicitudDetalles.Add(ronda);
        await db.SaveChangesAsync();

        await RecalcularEstadoAsync(db, solicitudId);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public async Task ActualizarEstadoRondaAsync(int rondaId, string nuevoEstado)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        await using var tx = await db.Database.BeginTransactionAsync();

        var ronda = await db.SolicitudDetalles.FindAsync(rondaId);
        if (ronda is null) return;
        ronda.Estado = nuevoEstado;
        await db.SaveChangesAsync();

        await RecalcularEstadoAsync(db, ronda.SolicitudId);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    private static async Task RecalcularEstadoAsync(ApplicationDbContext db, int solicitudId)
    {
        var solicitud = await db.Solicitudes.FindAsync(solicitudId);
        if (solicitud is null) return;
        var hayRondaAbierta = await db.SolicitudDetalles.AnyAsync(d => d.SolicitudId == solicitudId && d.Estado != "Cerrada");
        solicitud.Estado = hayRondaAbierta ? "Activa" : "Cerrada";
    }
}
