namespace dashboard.Models.Reclutamiento;

public class SolicitudDetalle
{
    public int Id { get; set; }
    public int SolicitudId { get; set; }
    public string ReclutadorId { get; set; } = string.Empty;
    public int CantidadPersonalSolicitado { get; set; }
    public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;
    public string Estado { get; set; } = "Pendiente"; // Pendiente / EnProceso / Cerrada
    public string? Observaciones { get; set; }
}
