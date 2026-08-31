namespace dashboard.Models.Reclutamiento;

public class PostulanteSolicitud
{
    public int Id { get; set; }
    public int PostulanteId { get; set; }
    public int SolicitudId { get; set; }
    public string Origen { get; set; } = string.Empty; // Directa / CargaMasiva
    public DateTime FechaIngreso { get; set; }
    public int? Puntaje { get; set; }
    public bool SeleccionadoPreseleccion { get; set; }
    public string? EtapaPreseleccion { get; set; } // Preseleccionado / Seleccionado
    public int? PuntajeInicial { get; set; }
    public int? PuntajeFinal { get; set; }

    // Carga masiva
    public int? PublicacionId { get; set; }
    public string? CanalOrigen { get; set; }
    public string? SituacionActual { get; set; }
    public DateTime? UltimaActualizacionCV { get; set; }
    public DateTime? UltimoLoginPortal { get; set; }
    public int? AdecuacionPortal { get; set; }
    public string? RespuestasPreguntasPortal { get; set; }

    // Evaluación
    public string? EtapaEvaluacion { get; set; } // Validado / Rechazado
    public string? AprobadoPorSupervisorId { get; set; }
    public DateTime? FechaEvaluacion { get; set; }
    public string? ComentarioEvaluacion { get; set; }
}
