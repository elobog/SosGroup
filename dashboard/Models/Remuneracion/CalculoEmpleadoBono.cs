namespace dashboard.Models.Remuneracion;

// Bono cargado por SOPR en la app y enviado (POST) a Talana — depende de la asistencia ya validada.
public class CalculoEmpleadoBono
{
    public int Id { get; set; }
    public int CalculoEmpleadoId { get; set; }
    public string TipoBono { get; set; } = string.Empty;
    public decimal Monto { get; set; }

    public DateTime FechaCarga { get; set; } = DateTime.UtcNow;
    public string CargadoPorUsuarioId { get; set; } = string.Empty;

    public string EstadoEnvio { get; set; } = "Pendiente"; // Pendiente / Enviado / Error
    public DateTime? FechaEnvioTalana { get; set; }
    public string? DetalleError { get; set; }
}
