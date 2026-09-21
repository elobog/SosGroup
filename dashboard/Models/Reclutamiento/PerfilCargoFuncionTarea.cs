namespace dashboard.Models.Reclutamiento;

// Cada bullet puntual dentro de una PerfilCargoFuncionCategoria.
public class PerfilCargoFuncionTarea
{
    public int Id { get; set; }
    public int PerfilCargoFuncionCategoriaId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public int Orden { get; set; }
}
