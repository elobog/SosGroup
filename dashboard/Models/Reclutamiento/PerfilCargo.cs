namespace dashboard.Models.Reclutamiento;

public class PerfilCargo
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Requisitos { get; set; }
    public string Estado { get; set; } = "Activo";
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
