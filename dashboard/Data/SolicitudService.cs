using Microsoft.EntityFrameworkCore;
using dashboard.Models.Reclutamiento;

namespace dashboard.Data;

// Mismo patrón que ClientesService/AccesoAppService: un DbContext propio por consulta.
public class SolicitudService(IDbContextFactory<ApplicationDbContext> dbFactory)
{
    // Roles cuya visibilidad se acota a los clientes asignados (ClienteReclutador/ClienteSupervisor).
    // SuperAdmin/Admin/Supervisor Administrativo ven todo, sin filtro (BD_Dashboard.md § Identity — Acceso).
    private static readonly string[] RolesAcotados = ["Reclutador", "Supervisor Operaciones"];

    public record SolicitudResumen(int Id, string CodigoSolicitud, string ClienteRazonSocial, string PerfilCargoNombre, DateTime FechaInicioServicio, string SupervisorNombre, string Estado);

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
        return resultado.Select(x => new SolicitudResumen(x.s.Id, x.s.CodigoSolicitud, x.RazonSocial, x.PerfilNombre, x.s.FechaInicioServicio, x.SupervisorNombre, x.s.Estado)).ToList();
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
