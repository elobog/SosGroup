namespace dashboard.Models.Reclutamiento;

public class PostulacionCandidato
{
    public int Id { get; set; }
    public int PublicacionId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string RUT { get; set; } = string.Empty;
    public string? Correo { get; set; }
    public string? Telefono { get; set; }
    public DateTime FechaHora { get; set; } = DateTime.UtcNow;
}
