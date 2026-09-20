namespace dashboard.Models.Remuneracion;

public class ContratoServicio
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty; // Outsourcing / ServiciosTransitorios / SeleccionYReclutamiento
    public DateOnly FechaInicio { get; set; }
    public DateOnly? FechaTermino { get; set; }
    public string Estado { get; set; } = "Activo"; // Activo / Terminado (baja lógica)
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Código que identifica este contrato en Talana — la Fase 0 de Base Talana lo usa para
    // homologar automáticamente y traer la dotación real; sin match, el contrato queda excluido
    // de la generación masiva de Cálculos hasta corregirlo.
    public string CodigoTalana { get; set; } = string.Empty;
    public bool Homologado { get; set; }
    public DateTime? FechaUltimaVerificacionHomologacion { get; set; }
}
