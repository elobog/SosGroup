namespace dashboard.Models.Reclutamiento;

public class EntrevistaPreseleccion
{
    public int Id { get; set; }
    public int PostulanteSolicitudId { get; set; } // único — 1 entrevista por preseleccionado
    public DateTime FechaHora { get; set; }
    public string? Checklist { get; set; } // JSON
    public string? Preguntas { get; set; } // JSON
    public string? Cierre { get; set; }
    public bool Cerrada { get; set; }
    public string? ReclutadorId { get; set; }
}
