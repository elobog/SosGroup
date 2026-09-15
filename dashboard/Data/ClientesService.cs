using Microsoft.EntityFrameworkCore;
using dashboard.Models.Admin;

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
}
