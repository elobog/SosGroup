namespace dashboard.Models;

// Genérica, no pertenece a una sola app — referencia por Entidad+EntidadId, no FK real
public class LogActividad
{
    public int Id { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public string Entidad { get; set; } = string.Empty;
    public int EntidadId { get; set; }
    public DateTime FechaHora { get; set; } = DateTime.UtcNow;
}
