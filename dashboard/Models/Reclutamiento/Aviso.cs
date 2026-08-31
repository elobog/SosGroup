namespace dashboard.Models.Reclutamiento;

public class Aviso
{
    public int Id { get; set; }
    public int AperturaId { get; set; }
    public int NumeroVersion { get; set; }
    public string TituloCargo { get; set; } = string.Empty;
    public string Modalidad { get; set; } = string.Empty; // Presencial / Híbrido / Remoto
    public string? Ubicacion { get; set; }
    public DateTime? CierrePostulaciones { get; set; }
    public string CuerpoHtml { get; set; } = string.Empty;
    public string CreadoPorUsuarioId { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
