namespace dashboard.Models.Remuneracion;

// Digitalización del Excel de estructura de renta por cliente/cargo que hoy vive en SharePoint —
// se importa desde ese mismo Excel, la mantiene Supervisor Remuneraciones (N1). Se usa para el
// cruce automático contra el libro de remuneraciones de Talana (estado CruceRenta del Cálculo).
public class EstructuraRenta
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string Cargo { get; set; } = string.Empty;
    public decimal SueldoBase { get; set; }
    public decimal? Gratificacion { get; set; }
    public DateOnly FechaVigencia { get; set; }
    public string ImportadoPorUsuarioId { get; set; } = string.Empty;
    public DateTime FechaImportacion { get; set; } = DateTime.UtcNow;
}
