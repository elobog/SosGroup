namespace dashboard.Models.Reclutamiento;

public class CredencialPlataforma
{
    public int Id { get; set; }
    public string? UsuarioId { get; set; } // null = credencial de empresa
    public string Plataforma { get; set; } = string.Empty;
    public bool Conectado { get; set; }
    public string? NombreCuenta { get; set; }
}
