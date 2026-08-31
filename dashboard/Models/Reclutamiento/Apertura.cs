namespace dashboard.Models.Reclutamiento;

public class Apertura
{
    public int Id { get; set; }
    public int SolicitudId { get; set; } // único — 1 Apertura por Solicitud
    public DateTime FechaInicio { get; set; } = DateTime.UtcNow;
    public string IniciadoPorUsuarioId { get; set; } = string.Empty;
    public string Estado { get; set; } = "Iniciada";
}
