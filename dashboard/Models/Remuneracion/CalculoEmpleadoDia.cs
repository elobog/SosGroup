namespace dashboard.Models.Remuneracion;

public class CalculoEmpleadoDia
{
    public int Id { get; set; }
    public int CalculoEmpleadoId { get; set; }
    public DateOnly Fecha { get; set; }

    // Trabajado / Libre / Ausencia / LicenciaMedica / Vacaciones / PermisoConGoce / PermisoSinGoce
    public string TipoJornada { get; set; } = "Libre";

    public decimal? HorasExtra { get; set; } // solo aplica si TipoJornada == Trabajado

    // GeoVictoria / Talana / Workera / LibroEstandarizado — de dónde vino este dato de asistencia.
    // Licencias médicas y vacaciones siempre vienen con Origen = Talana (no se corrigen desde acá).
    public string Origen { get; set; } = string.Empty;
}
