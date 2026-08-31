namespace dashboard.Models.Reclutamiento;

// v1 aún vigente, pendiente de integrar al flujo activo de Preselección
public class DocumentoExtraccionIA
{
    public int Id { get; set; }
    public int PostulanteDocumentoId { get; set; }
    public string? TextoExtraido { get; set; }
    public string? DatosEstructurados { get; set; } // JSON
    public DateTime? FechaProcesamiento { get; set; }
    public string Estado { get; set; } = "Pendiente"; // Pendiente/Procesando/Listo/Error
}
