namespace dashboard.Models.Remuneracion;

// Trazabilidad de correcciones sobre un día de asistencia — quién, cuándo, qué cambió.
public class CalculoEmpleadoDiaHistorial
{
    public int Id { get; set; }
    public int CalculoEmpleadoDiaId { get; set; }
    public DateTime FechaHora { get; set; } = DateTime.UtcNow;
    public string UsuarioId { get; set; } = string.Empty;
    public string CampoModificado { get; set; } = string.Empty; // TipoJornada / HorasExtra
    public string? ValorAnterior { get; set; }
    public string? ValorNuevo { get; set; }
}
