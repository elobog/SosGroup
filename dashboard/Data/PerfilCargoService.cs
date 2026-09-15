using Microsoft.EntityFrameworkCore;
using dashboard.Models.Reclutamiento;

namespace dashboard.Data;

// Mismo patrón que ClientesService/AccesoAppService: un DbContext propio por consulta.
public class PerfilCargoService(IDbContextFactory<ApplicationDbContext> dbFactory)
{
    public static readonly string[] RolesAprobadores = ["Supervisor Operaciones", "Supervisor Administrativo"];

    public record PerfilCargoResumen(int Id, int ClienteId, string ClienteRazonSocial, string Nombre, string Estado, string? CondicionVigenteResumen);

    public async Task<List<PerfilCargoResumen>> ListarAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var perfiles = await (from p in db.PerfilesCargo
                               join c in db.Clientes on p.ClienteId equals c.Id
                               orderby p.Nombre
                               select new { p.Id, p.ClienteId, c.RazonSocial, p.Nombre, p.Estado }).ToListAsync();

        var vigentes = await db.PerfilCargoVersiones
            .Where(v => v.Estado == "Vigente")
            .ToListAsync();

        return perfiles.Select(p =>
        {
            var vigente = vigentes.FirstOrDefault(v => v.PerfilCargoId == p.Id);
            var resumen = vigente is null ? null : $"{vigente.RentaFija ?? "—"}";
            return new PerfilCargoResumen(p.Id, p.ClienteId, p.RazonSocial, p.Nombre, p.Estado, resumen);
        }).ToList();
    }

    public record VersionVigenteOpcion(int PerfilCargoVersionId, int PerfilCargoId, string PerfilCargoNombre, string? RentaFija);

    public async Task<List<VersionVigenteOpcion>> ListarVigentesPorClienteAsync(int clienteId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await (from pc in db.PerfilesCargo
                       join pv in db.PerfilCargoVersiones on pc.Id equals pv.PerfilCargoId
                       where pc.ClienteId == clienteId && pc.Estado == "Activo" && pv.Estado == "Vigente"
                       select new VersionVigenteOpcion(pv.Id, pc.Id, pc.Nombre, pv.RentaFija)).ToListAsync();
    }

    public async Task<PerfilCargo?> ObtenerAsync(int id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.PerfilesCargo.FindAsync(id);
    }

    public async Task<string?> ObtenerNombrePorVersionAsync(int versionId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await (from pv in db.PerfilCargoVersiones
                       join pc in db.PerfilesCargo on pv.PerfilCargoId equals pc.Id
                       where pv.Id == versionId
                       select pc.Nombre).FirstOrDefaultAsync();
    }

    public record VersionConAprobaciones(PerfilCargoVersion Version, List<PerfilCargoVersionAprobacion> Aprobaciones);

    public async Task<List<VersionConAprobaciones>> ListarVersionesAsync(int perfilCargoId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var versiones = await db.PerfilCargoVersiones.Where(v => v.PerfilCargoId == perfilCargoId).OrderByDescending(v => v.NumeroVersion).ToListAsync();
        var versionIds = versiones.Select(v => v.Id).ToList();
        var aprobaciones = await db.PerfilCargoVersionAprobaciones.Where(a => versionIds.Contains(a.PerfilCargoVersionId)).ToListAsync();
        return versiones.Select(v => new VersionConAprobaciones(v, aprobaciones.Where(a => a.PerfilCargoVersionId == v.Id).ToList())).ToList();
    }

    public async Task<int> CrearAsync(PerfilCargo perfil, PerfilCargoVersion primeraVersion)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        await using var tx = await db.Database.BeginTransactionAsync();

        db.PerfilesCargo.Add(perfil);
        await db.SaveChangesAsync();

        primeraVersion.PerfilCargoId = perfil.Id;
        primeraVersion.NumeroVersion = 1;
        primeraVersion.Estado = "Pendiente";
        db.PerfilCargoVersiones.Add(primeraVersion);
        await db.SaveChangesAsync();

        foreach (var rol in RolesAprobadores)
        {
            db.PerfilCargoVersionAprobaciones.Add(new PerfilCargoVersionAprobacion { PerfilCargoVersionId = primeraVersion.Id, Rol = rol });
        }
        await db.SaveChangesAsync();

        await tx.CommitAsync();
        return perfil.Id;
    }

    public async Task ActualizarAsync(PerfilCargo perfil)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.PerfilesCargo.Update(perfil);
        await db.SaveChangesAsync();
    }

    public async Task AgregarCondicionAsync(int perfilCargoId, PerfilCargoVersion nuevaVersion)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var ultimaVersion = await db.PerfilCargoVersiones.Where(v => v.PerfilCargoId == perfilCargoId).OrderByDescending(v => v.NumeroVersion).FirstAsync();

        await using var tx = await db.Database.BeginTransactionAsync();

        nuevaVersion.PerfilCargoId = perfilCargoId;
        nuevaVersion.NumeroVersion = ultimaVersion.NumeroVersion + 1;
        nuevaVersion.Estado = "Pendiente";
        nuevaVersion.VersionBaseId = ultimaVersion.Id;
        db.PerfilCargoVersiones.Add(nuevaVersion);
        await db.SaveChangesAsync();

        foreach (var rol in RolesAprobadores)
        {
            db.PerfilCargoVersionAprobaciones.Add(new PerfilCargoVersionAprobacion { PerfilCargoVersionId = nuevaVersion.Id, Rol = rol });
        }
        await db.SaveChangesAsync();

        await tx.CommitAsync();
    }

    // Devuelve true si la versión quedó Vigente tras esta aprobación.
    public async Task<bool> AprobarVersionAsync(int versionId, string rol, string usuarioId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        await using var tx = await db.Database.BeginTransactionAsync();

        var aprobacion = await db.PerfilCargoVersionAprobaciones.FirstOrDefaultAsync(a => a.PerfilCargoVersionId == versionId && a.Rol == rol);
        if (aprobacion is null || aprobacion.AprobadoPorUsuarioId is not null) return false;

        aprobacion.AprobadoPorUsuarioId = usuarioId;
        aprobacion.FechaAprobacion = DateTime.UtcNow;
        await db.SaveChangesAsync();

        var todasAprobadas = !await db.PerfilCargoVersionAprobaciones.AnyAsync(a => a.PerfilCargoVersionId == versionId && a.AprobadoPorUsuarioId == null);
        if (todasAprobadas)
        {
            var version = await db.PerfilCargoVersiones.FirstAsync(v => v.Id == versionId);
            var vigenteAnterior = await db.PerfilCargoVersiones.FirstOrDefaultAsync(v => v.PerfilCargoId == version.PerfilCargoId && v.Estado == "Vigente");
            if (vigenteAnterior is not null) vigenteAnterior.Estado = "Historica";
            version.Estado = "Vigente";
            await db.SaveChangesAsync();
        }

        await tx.CommitAsync();
        return todasAprobadas;
    }
}
