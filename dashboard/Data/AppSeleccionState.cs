namespace dashboard.Data;

// Estado compartido (scoped al circuito) entre el topbar y el sidebar, que son dos islas
// interactivas independientes dentro del mismo layout estático — ver AppShellLayout.razor.
// Solo el topbar consulta la base de datos (UserManager/AccesoAppService); el sidebar es un
// consumidor puramente en memoria de este estado — dos componentes interactivos hermanos que
// consultaran la BD al mismo tiempo compartirían el mismo DbContext scoped del circuito y EF
// Core no soporta eso (ver bug ya documentado al conectar AppShellLayout con las páginas).
public class AppSeleccionState
{
    public string? AppActual { get; private set; }
    public Dictionary<string, List<string>> Acceso { get; private set; } = [];

    public event Action? OnChange;

    public void Inicializar(Dictionary<string, List<string>> acceso, string? appInicial)
    {
        Acceso = acceso;
        AppActual = appInicial;
        OnChange?.Invoke();
    }

    public void Seleccionar(string app)
    {
        AppActual = app;
        OnChange?.Invoke();
    }
}
