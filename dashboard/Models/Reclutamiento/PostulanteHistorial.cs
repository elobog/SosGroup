namespace dashboard.Models.Reclutamiento;

// v1 aún vigente, pendiente de integrar al flujo activo de Preselección
public class PostulanteHistorial
{
    public int Id { get; set; }
    public int PostulanteId { get; set; }
    public string Tipo { get; set; } = string.Empty; // Experiencia / Educacion / Otro
    public string? Descripcion { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public string Origen { get; set; } = string.Empty; // Manual / IA
}
