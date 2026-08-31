namespace dashboard.Models.Admin;

// PK compuesta (RoleId, App, Modulo)
public class PerfilModulo
{
    public string RoleId { get; set; } = string.Empty;
    public string App { get; set; } = string.Empty;
    public string Modulo { get; set; } = string.Empty;
}
