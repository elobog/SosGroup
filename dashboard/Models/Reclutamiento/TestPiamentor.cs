namespace dashboard.Models.Reclutamiento;

public class TestPiamentor
{
    public int Id { get; set; }
    public int PostulanteSolicitudId { get; set; }
    public string ReclutadorId { get; set; } = string.Empty;
    public string Proveedor { get; set; } = "Piamentor.cl";
    public decimal CostoUsd { get; set; }
    public int Puntaje { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}
