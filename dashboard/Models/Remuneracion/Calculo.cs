namespace dashboard.Models.Remuneracion;

public class Calculo
{
    public int Id { get; set; }
    public int ContratoServicioId { get; set; }
    public DateOnly Periodo { get; set; } // primer día del mes calculado
    public string Estado { get; set; } = "Abierto"; // Abierto / Enviado / Cerrado
    public DateTime? FechaEnvio { get; set; }
    public DateTime? FechaCierre { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
