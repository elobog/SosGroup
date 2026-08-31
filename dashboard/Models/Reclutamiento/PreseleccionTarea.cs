namespace dashboard.Models.Reclutamiento;

public class PreseleccionTarea
{
    public int Id { get; set; }
    public int PostulanteSolicitudId { get; set; }
    // Inicio/EnvioCorreo/UploadDocumentos/TestPiamentor/ValidacionIA1/Entrevista/CertificadoWHO/ValidacionIA2/Validacion
    public string Tipo { get; set; } = string.Empty;
    public string Estado { get; set; } = "Pendiente"; // Pendiente / Completado
    public DateTime? Fecha { get; set; }
    public string? Detalle { get; set; }
}
