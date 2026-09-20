namespace dashboard.Models.Reclutamiento;

public class Aviso
{
    public int Id { get; set; }
    public int AperturaId { get; set; }
    public int NumeroVersion { get; set; }
    public string TituloCargo { get; set; } = string.Empty;
    public string Modalidad { get; set; } = string.Empty; // Presencial / Híbrido / Remoto
    public DateTime? CierrePostulaciones { get; set; }
    public string CuerpoHtml { get; set; } = string.Empty;
    public string CreadoPorUsuarioId { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Campos estructurados que piden los portales de empleo al publicar (Computrabajo, Trabajando.com)
    // — verificados contra avisos reales de esos sitios, ninguno es obligatorio porque cada plataforma
    // pide un subconjunto distinto (ver Reclutamiento2.md).
    public string? Region { get; set; }
    public string? Comuna { get; set; }
    public string? NivelCargo { get; set; } // "Ayudante", "Encargado", etc.
    public string? TipoContrato { get; set; }
    public string? JornadaTipo { get; set; } // "Completa" / "Parcial"
    public string? JornadaTexto { get; set; } // "42 horas semanales"
    public string? TurnoTexto { get; set; } // "2x2 07:00 a 19:00"
    public decimal? SalarioPublicado { get; set; }
    public string? SalarioPeriodicidad { get; set; }
    public string? EducacionMinima { get; set; }
    public string? EducacionEstado { get; set; } // "Graduado" / "EnCurso"
    public int? AniosExperiencia { get; set; }
    public string? CursoDeseable { get; set; }
    public int? EdadMinima { get; set; }
    public int? EdadMaxima { get; set; }
    public bool EmpleoInclusivo { get; set; }
    public string? PalabrasClave { get; set; }
}
