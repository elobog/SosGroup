namespace dashboard.Models.Reclutamiento;

public class InteraccionIA
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public int? SolicitudId { get; set; }
    public int? PostulanteId { get; set; }
    public int? PerfilCargoVersionId { get; set; }
    public string? ReclutadorId { get; set; }
    public string Proveedor { get; set; } = string.Empty; // "Azure OpenAI"
    public string Proposito { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int TokensEntrada { get; set; }
    public int TokensSalida { get; set; }
    public decimal CostoUsd { get; set; }
    public DateTime FechaHora { get; set; } = DateTime.UtcNow;
}
