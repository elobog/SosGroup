namespace dashboard.Models.Reclutamiento;

public class PerfilCargoVersionAprobacion
{
    public int Id { get; set; }
    public int PerfilCargoVersionId { get; set; }
    public string Rol { get; set; } = string.Empty; // 'Supervisor Operaciones' / 'Supervisor Administrativo'
    public string? AprobadoPorUsuarioId { get; set; }
    public DateTime? FechaAprobacion { get; set; }
}
