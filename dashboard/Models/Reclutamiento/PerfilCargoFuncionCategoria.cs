namespace dashboard.Models.Reclutamiento;

// Ej. "Tareas operativas de producción" — agrupa las tareas puntuales de PerfilCargoFuncionTarea.
public class PerfilCargoFuncionCategoria
{
    public int Id { get; set; }
    public int PerfilCargoVersionId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Orden { get; set; }
}
