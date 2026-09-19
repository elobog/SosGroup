namespace dashboard.Models.Remuneracion;

public class CalculoEmpleadoDia
{
    public int Id { get; set; }
    public int CalculoEmpleadoId { get; set; }
    public DateOnly Fecha { get; set; }

    // Trabajado / Libre / Ausencia / LicenciaMedica / Vacaciones / PermisoConGoce / PermisoSinGoce
    public string TipoJornada { get; set; } = "Libre";

    public decimal? HorasExtra { get; set; } // solo aplica si TipoJornada == Trabajado
}
