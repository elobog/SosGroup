namespace dashboard.Models.Remuneracion;

// BaseTalana — leído desde la API de Talana, no se crea/edita manualmente (ver remuneraciones.md).
public class EmpleadoTalana
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string RUT { get; set; } = string.Empty;
    public int ClienteId { get; set; }
    public int? ContratoServicioId { get; set; }
    public string? Cargo { get; set; }
    public string EstadoTalana { get; set; } = "Activo"; // Activo / Inactivo, según lo informado por Talana
    public DateOnly? FechaIngreso { get; set; }
    public DateTime? FechaUltimaSincronizacion { get; set; }
}
