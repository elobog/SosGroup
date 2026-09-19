namespace dashboard.Models.Remuneracion;

public class AuditoriaEjecucion
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string Tipo { get; set; } = "Automatica"; // Automatica / Manual
    public DateOnly MesAuditado { get; set; }
    public int ContratosRevisados { get; set; }
}
