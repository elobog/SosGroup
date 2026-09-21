namespace dashboard.Models.Reclutamiento;

// El PDF que el cliente entrega con su propio Perfil de Cargo, tal cual — guardado en Blob
// Storage (stsosgroupapp/perfiles-cargo). Sirve de referencia y como entrada para el análisis IA.
public class PerfilCargoDocumentoOriginal
{
    public int Id { get; set; }
    public int PerfilCargoVersionId { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaBlob { get; set; } = string.Empty;
    public string CargadoPorUsuarioId { get; set; } = string.Empty;
    public DateTime FechaCarga { get; set; } = DateTime.UtcNow;
}
