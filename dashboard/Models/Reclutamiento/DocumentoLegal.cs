namespace dashboard.Models.Reclutamiento;

public class DocumentoLegal
{
    public int Id { get; set; }
    public int PostulanteSolicitudId { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string TipoArchivo { get; set; } = string.Empty; // PDF / Imagen
    public string Origen { get; set; } = string.Empty; // Reclutador / Postulante
    public string RutaArchivo { get; set; } = string.Empty;
    public DateTime FechaCarga { get; set; } = DateTime.UtcNow;
}
