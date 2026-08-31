namespace dashboard.Models.Admin;

public class Cliente
{
    public int Id { get; set; }
    public string RazonSocial { get; set; } = string.Empty;
    public string RUT { get; set; } = string.Empty;
    public string? ContactoNombre { get; set; }
    public string? ContactoCorreo { get; set; }
    public string? ContactoTelefono { get; set; }
    public string Estado { get; set; } = "Activo"; // Activo / Inactivo (bloqueado)
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
