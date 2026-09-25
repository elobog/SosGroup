using Microsoft.EntityFrameworkCore;

namespace dashboard.Data;

public class CostosIAService(IDbContextFactory<ApplicationDbContext> dbFactory)
{
    public record Resumen(int Llamadas, long TokensEntrada, long TokensSalida, decimal CostoUsd);
    public record Grupo(string Nombre, int Llamadas, long Tokens, decimal CostoUsd);
    public record Detalle(DateTime FechaHora, string Proposito, string Modelo, string Cliente, string Usuario, int TokensEntrada, int TokensSalida, decimal CostoUsd);
    public record Reporte(Resumen Total, List<Grupo> PorProposito, List<Grupo> PorCliente, List<Grupo> PorUsuario, List<Detalle> Ultimas);

    public async Task<Reporte> ObtenerAsync(DateTime? desdeUtc)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        var query = db.InteraccionesIA.AsNoTracking().AsQueryable();
        if (desdeUtc is not null) query = query.Where(i => i.FechaHora >= desdeUtc);

        var filas = await query.OrderByDescending(i => i.FechaHora).ToListAsync();

        var clientes = await db.Clientes.AsNoTracking().ToDictionaryAsync(c => c.Id, c => c.RazonSocial);
        var usuarios = await db.Users.AsNoTracking().ToDictionaryAsync(u => u.Id, u => u.Nombre);

        string NombreCliente(int id) => clientes.GetValueOrDefault(id, $"Cliente {id}");
        string NombreUsuario(string? id) => id is null ? "(sin usuario)" : usuarios.GetValueOrDefault(id, "(usuario eliminado)");

        var total = new Resumen(filas.Count, filas.Sum(f => (long)f.TokensEntrada), filas.Sum(f => (long)f.TokensSalida), filas.Sum(f => f.CostoUsd));

        static List<Grupo> Agrupar<T>(IEnumerable<T> src, Func<T, string> clave, Func<T, int> tokens, Func<T, decimal> costo) =>
            src.GroupBy(clave)
               .Select(g => new Grupo(g.Key, g.Count(), g.Sum(x => (long)tokens(x)), g.Sum(costo)))
               .OrderByDescending(g => g.CostoUsd)
               .ToList();

        return new Reporte(
            total,
            Agrupar(filas, f => f.Proposito, f => f.TokensEntrada + f.TokensSalida, f => f.CostoUsd),
            Agrupar(filas, f => NombreCliente(f.ClienteId), f => f.TokensEntrada + f.TokensSalida, f => f.CostoUsd),
            Agrupar(filas, f => NombreUsuario(f.ReclutadorId), f => f.TokensEntrada + f.TokensSalida, f => f.CostoUsd),
            filas.Take(50).Select(f => new Detalle(f.FechaHora, f.Proposito, f.Modelo, NombreCliente(f.ClienteId), NombreUsuario(f.ReclutadorId), f.TokensEntrada, f.TokensSalida, f.CostoUsd)).ToList());
    }
}
