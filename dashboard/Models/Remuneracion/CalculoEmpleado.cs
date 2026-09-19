namespace dashboard.Models.Remuneracion;

// Una fila por persona dentro de un Calculo — los totales (días trabajados, ausencias, horas
// extra, etc.) se derivan de CalculoEmpleadoDia, no se guardan acá (ver remuneraciones.md).
public class CalculoEmpleado
{
    public int Id { get; set; }
    public int CalculoId { get; set; }
    public int EmpleadoTalanaId { get; set; }
}
