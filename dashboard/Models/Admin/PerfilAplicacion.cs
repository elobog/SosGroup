namespace dashboard.Models.Admin;

// PK compuesta (RoleId, App) — referencia AspNetRoles.Id directo, no duplica el concepto de rol/perfil
public class PerfilAplicacion
{
    public string RoleId { get; set; } = string.Empty;
    public string App { get; set; } = string.Empty; // "Reclutamiento" / "Remuneracion" / "Admin"
}
