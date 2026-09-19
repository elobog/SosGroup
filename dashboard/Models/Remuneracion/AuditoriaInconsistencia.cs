namespace dashboard.Models.Remuneracion;

public class AuditoriaInconsistencia
{
    public int Id { get; set; }
    public int AuditoriaEjecucionId { get; set; }
    public int CalculoEmpleadoId { get; set; }
    public string TipoInconsistencia { get; set; } = string.Empty;
    public string Detalle { get; set; } = string.Empty;
    public string Estado { get; set; } = "Pendiente"; // Pendiente / Resuelta
}
