using Microsoft.AspNetCore.Identity;

namespace dashboard.Data;

// Ver BD_Dashboard.md > Admin > ApplicationUser (extiende AspNetUsers)
public class ApplicationUser : IdentityUser
{
    public string Nombre { get; set; } = string.Empty;

    // Activo / Bloqueado
    public string Estado { get; set; } = "Activo";

    public DateTime? UltimaVerificacion2FA { get; set; }
}
