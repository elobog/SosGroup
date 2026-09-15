namespace dashboard.Data;

// Universo fijo de apps/módulos del portal (ver admin.md / Reclutamiento2.md) — no se calcula desde la BD
// porque la matriz de Accesos debe poder mostrar/habilitar un módulo aunque ningún perfil lo tenga marcado todavía.
public static class PortalDefinicion
{
    public static readonly (string App, (string Id, string Label)[] Modulos)[] Apps =
    [
        ("Admin", [("Usuarios", "Usuarios"), ("Perfiles", "Perfiles"), ("Accesos", "Accesos"), ("Clientes", "Clientes"), ("CostosIA", "Costos IA")]),
        ("Reclutamiento", [("Solicitud", "Solicitud"), ("Preseleccion", "Preselección"), ("Seleccion", "Selección"), ("Ingreso", "Ingreso")]),
    ];

    // Módulos con pantalla real construida — el resto se muestra bloqueado en el sidebar aunque el perfil tenga acceso.
    public static readonly Dictionary<string, HashSet<string>> ModulosConstruidos = new()
    {
        ["Admin"] = ["Clientes", "Usuarios", "Perfiles", "Accesos"],
        ["Reclutamiento"] = ["Solicitud"],
    };

    public static readonly Dictionary<string, string> RutaModulo = new()
    {
        ["Admin:Clientes"] = "/Admin/Clientes",
        ["Admin:Usuarios"] = "/Admin/Usuarios",
        ["Admin:Perfiles"] = "/Admin/Perfiles",
        ["Admin:Accesos"] = "/Admin/Accesos",
        ["Reclutamiento:Solicitud"] = "/Reclutamiento/Solicitud",
    };

    public static string EtiquetaModulo(string app, string moduloId) =>
        Apps.FirstOrDefault(a => a.App == app).Modulos.FirstOrDefault(m => m.Id == moduloId).Label ?? moduloId;

    // Orden fijo del sidebar — no alfabético.
    public static int OrdenModulo(string app, string moduloId)
    {
        var modulos = Apps.FirstOrDefault(a => a.App == app).Modulos;
        var idx = Array.FindIndex(modulos ?? [], m => m.Id == moduloId);
        return idx < 0 ? int.MaxValue : idx;
    }
}
