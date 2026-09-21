namespace dashboard.Models.Remuneracion;

// Bonos esperados dentro de una EstructuraRenta — contra esto se cruzan los bonos reales cargados.
public class EstructuraRentaBono
{
    public int Id { get; set; }
    public int EstructuraRentaId { get; set; }
    public string TipoBono { get; set; } = string.Empty;
    public decimal MontoEsperado { get; set; }
}
