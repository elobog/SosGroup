namespace dashboard.Models.Reclutamiento;

public class CertificadoWHO
{
    public int Id { get; set; }
    public int PostulanteSolicitudId { get; set; }
    public string ReclutadorId { get; set; } = string.Empty;
    public string Proveedor { get; set; } = "WHO.cl";
    public decimal CostoUsd { get; set; }
    public string Resultado { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}
