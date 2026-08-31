namespace dashboard.Models.Reclutamiento;

public class PerfilCargoVersion
{
    public int Id { get; set; }
    public int PerfilCargoId { get; set; }
    public int NumeroVersion { get; set; }
    public string? RentaFija { get; set; }
    public string? RentaVariable { get; set; }
    public string? Beneficios { get; set; }
    public DateTime FechaVigencia { get; set; }
    public string Estado { get; set; } = "Pendiente"; // Pendiente / Vigente / Historica
    public int? VersionBaseId { get; set; }
}
