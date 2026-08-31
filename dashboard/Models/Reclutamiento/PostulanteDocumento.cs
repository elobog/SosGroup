namespace dashboard.Models.Reclutamiento;

// v1 aún vigente, pendiente de integrar al flujo activo de Preselección
public class PostulanteDocumento
{
    public int Id { get; set; }
    public int PostulanteId { get; set; }
    public string Tipo { get; set; } = string.Empty; // CV / CertificadoAntecedentes / Otro
    public string RutaArchivo { get; set; } = string.Empty;
    public DateTime FechaCarga { get; set; }
    public string EstadoProcesamiento { get; set; } = "Pendiente"; // Pendiente/Procesando/Listo/Error
}
