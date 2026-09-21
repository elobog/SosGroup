using Microsoft.EntityFrameworkCore;
using dashboard.Models.Remuneracion;

namespace dashboard.Data;

// Mismo patrón que ClientesService/AccesoAppService: un DbContext propio por consulta.
public class ContratosService(IDbContextFactory<ApplicationDbContext> dbFactory)
{
    public record ContratoResumen(int Id, string Nombre, string ClienteRazonSocial, string Tipo, string CodigoTalana, bool Homologado, string Estado);

    public async Task<List<ContratoResumen>> ListarAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await (from c in db.ContratosServicio
                       join cl in db.Clientes on c.ClienteId equals cl.Id
                       orderby c.Nombre
                       select new ContratoResumen(c.Id, c.Nombre, cl.RazonSocial, c.Tipo, c.CodigoTalana, c.Homologado, c.Estado))
            .ToListAsync();
    }

    public async Task<ContratoServicio?> ObtenerAsync(int id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.ContratosServicio.FindAsync(id);
    }

    public async Task<int> CrearAsync(ContratoServicio contrato)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.ContratosServicio.Add(contrato);
        await db.SaveChangesAsync();
        return contrato.Id;
    }

    public async Task ActualizarAsync(ContratoServicio contrato)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.ContratosServicio.Update(contrato);
        await db.SaveChangesAsync();
    }
}
