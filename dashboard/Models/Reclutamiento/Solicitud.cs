namespace dashboard.Models.Reclutamiento;

public class Solicitud
{
    public int Id { get; set; }
    public string CodigoSolicitud { get; set; } = string.Empty;
    public int ClienteId { get; set; }
    public int PerfilCargoVersionId { get; set; }
    public DateTime FechaInicioServicio { get; set; }
    public int? SucursalId { get; set; }
    public string? DireccionServicio { get; set; } // override cuando no hay SucursalId
    public string SupervisorId { get; set; } = string.Empty; // usuario rol Supervisor Operaciones
    public string Estado { get; set; } = "Activa"; // derivado
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
