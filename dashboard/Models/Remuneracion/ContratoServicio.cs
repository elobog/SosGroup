namespace dashboard.Models.Remuneracion;

public class ContratoServicio
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty; // Outsourcing / ServiciosTransitorios / SeleccionYReclutamiento
    public DateOnly FechaInicio { get; set; }
    public DateOnly? FechaTermino { get; set; }
    public string Estado { get; set; } = "Activo"; // Activo / Terminado (baja lógica)
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
