namespace dashboard.Models.Reclutamiento;

// Acceso del candidato al portal público sin cuenta/contraseña: un link mágico por correo que
// cubre a la vez "confirmar el correo" y "aceptar la política de datos personales" (VerificacionInicial),
// o simplemente "volver a entrar" en una postulación futura (AccesoRetorno). Antes de que exista el
// Postulante (el RUT recién se pide en el paso siguiente), el contacto inicial vive acá.
public class PostulanteAccesoToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public int SolicitudId { get; set; }
    public string NombreContacto { get; set; } = string.Empty;
    public string CorreoContacto { get; set; } = string.Empty;
    public string TelefonoContacto { get; set; } = string.Empty;
    public int? PostulanteId { get; set; }
    public string Proposito { get; set; } = string.Empty; // VerificacionInicial / AccesoRetorno
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaExpiracion { get; set; }
    public DateTime? UsadoEn { get; set; }
}
