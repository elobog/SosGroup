namespace dashboard.Models.Admin;

public class ClienteSucursal
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string Nombre { get; set; } = string.Empty; // ej. "Casa Matriz", "Planta Renca"
    public string Direccion { get; set; } = string.Empty;
    public string? ContactoResponsableNombre { get; set; }
    public string? ContactoResponsableCargo { get; set; }
    public string? ContactoResponsableCorreo { get; set; }
    public string? ContactoResponsableTelefono { get; set; }
    public string Estado { get; set; } = "Activo";
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
