namespace dashboard.Models.Reclutamiento;

public class Publicacion
{
    public int Id { get; set; }
    public int AvisoId { get; set; }
    public string Plataforma { get; set; } = string.Empty; // Computrabajo / Trabajando.com / LinkedIn / Facebook / Instagram / TikTok
    public string UsuarioId { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; } = DateTime.UtcNow;
    public string Metodo { get; set; } = string.Empty; // API / Manual
    public string Estado { get; set; } = string.Empty; // Publicado / Pendiente de carga manual / Error
    public int CantidadPostulantes { get; set; }
}
