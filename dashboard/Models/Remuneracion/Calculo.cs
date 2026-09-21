namespace dashboard.Models.Remuneracion;

public class Calculo
{
    public int Id { get; set; }
    public int ContratoServicioId { get; set; }
    public DateOnly Periodo { get; set; } // primer día del mes calculado

    // Abierto: SOPR carga/corrige asistencia y bonos (ciclo iterativo, se repite las veces que haga falta).
    // CruceRenta: RG revisa las inconsistencias contra EstructuraRenta (cruce automático).
    // Auditado: corrió la auditoría de asistencia y bonos.
    // Validado: RG valida en Talana — hito final, cierra el proceso.
    // Reprocesar (SOPR, o RG si implica alta de empleado/vacaciones/licencia) devuelve el Cálculo a Abierto.
    public string Estado { get; set; } = "Abierto";

    public DateTime? FechaCruceRenta { get; set; }
    public string? UsuarioCruceRentaId { get; set; }

    public DateTime? FechaValidacion { get; set; }
    public string? UsuarioValidacionId { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
