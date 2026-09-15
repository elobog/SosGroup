using Microsoft.EntityFrameworkCore;
using dashboard.Models.Admin;
using dashboard.Models.Reclutamiento;

namespace dashboard.Data;

// Mismo patrón que AccesoAppService: un DbContext propio por consulta (IDbContextFactory),
// independiente del DbContext scoped que usa Identity.
public class ClientesService(IDbContextFactory<ApplicationDbContext> dbFactory)
{
    public record ClienteResumen(int Id, string RazonSocial, string RUT, string? ContactoNombre, string Estado);

    public async Task<List<ClienteResumen>> ListarAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Clientes
            .OrderBy(c => c.RazonSocial)
            .Select(c => new ClienteResumen(c.Id, c.RazonSocial, c.RUT, c.ContactoNombre, c.Estado))
            .ToListAsync();
    }

    public async Task<Cliente?> ObtenerAsync(int id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Clientes.FindAsync(id);
    }

    public async Task<int> CrearAsync(Cliente cliente)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync();
        return cliente.Id;
    }

    public async Task ActualizarAsync(Cliente cliente)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.Clientes.Update(cliente);
        await db.SaveChangesAsync();
    }

    public async Task<List<ClienteSucursal>> ListarSucursalesAsync(int clienteId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.ClienteSucursales.Where(s => s.ClienteId == clienteId).OrderBy(s => s.Nombre).ToListAsync();
    }

    public async Task CrearSucursalAsync(ClienteSucursal sucursal)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.ClienteSucursales.Add(sucursal);
        await db.SaveChangesAsync();
    }

    public async Task ActualizarSucursalAsync(ClienteSucursal sucursal)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.ClienteSucursales.Update(sucursal);
        await db.SaveChangesAsync();
    }

    public record UsuarioAsignado(string UsuarioId, string Nombre, string Correo);

    public async Task<List<UsuarioAsignado>> ListarUsuariosPorRolAsync(string rol)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var roleId = await db.Roles.Where(r => r.Name == rol).Select(r => r.Id).FirstOrDefaultAsync();
        if (roleId is null) return [];

        return await (from ur in db.UserRoles
                       join u in db.Users on ur.UserId equals u.Id
                       where ur.RoleId == roleId
                       orderby u.Nombre
                       select new UsuarioAsignado(u.Id, u.Nombre, u.Email ?? "")).ToListAsync();
    }

    public async Task<List<UsuarioAsignado>> ListarReclutadoresAsync(int clienteId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await (from cr in db.ClienteReclutadores
                       join u in db.Users on cr.UsuarioId equals u.Id
                       where cr.ClienteId == clienteId
                       orderby u.Nombre
                       select new UsuarioAsignado(u.Id, u.Nombre, u.Email ?? "")).ToListAsync();
    }

    public async Task AsignarReclutadorAsync(int clienteId, string usuarioId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        if (await db.ClienteReclutadores.AnyAsync(cr => cr.ClienteId == clienteId && cr.UsuarioId == usuarioId)) return;
        db.ClienteReclutadores.Add(new ClienteReclutador { ClienteId = clienteId, UsuarioId = usuarioId });
        await db.SaveChangesAsync();
    }

    public async Task QuitarReclutadorAsync(int clienteId, string usuarioId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.ClienteReclutadores.RemoveRange(db.ClienteReclutadores.Where(cr => cr.ClienteId == clienteId && cr.UsuarioId == usuarioId));
        await db.SaveChangesAsync();
    }

    public async Task<List<UsuarioAsignado>> ListarSupervisoresAsync(int clienteId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await (from cs in db.ClienteSupervisores
                       join u in db.Users on cs.UsuarioId equals u.Id
                       where cs.ClienteId == clienteId
                       orderby u.Nombre
                       select new UsuarioAsignado(u.Id, u.Nombre, u.Email ?? "")).ToListAsync();
    }

    public async Task AsignarSupervisorAsync(int clienteId, string usuarioId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        if (await db.ClienteSupervisores.AnyAsync(cs => cs.ClienteId == clienteId && cs.UsuarioId == usuarioId)) return;
        db.ClienteSupervisores.Add(new ClienteSupervisor { ClienteId = clienteId, UsuarioId = usuarioId });
        await db.SaveChangesAsync();
    }

    public async Task QuitarSupervisorAsync(int clienteId, string usuarioId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.ClienteSupervisores.RemoveRange(db.ClienteSupervisores.Where(cs => cs.ClienteId == clienteId && cs.UsuarioId == usuarioId));
        await db.SaveChangesAsync();
    }
}
