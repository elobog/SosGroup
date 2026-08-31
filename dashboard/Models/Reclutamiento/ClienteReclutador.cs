namespace dashboard.Models.Reclutamiento;

// PK compuesta (ClienteId, UsuarioId)
public class ClienteReclutador
{
    public int ClienteId { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;
}
