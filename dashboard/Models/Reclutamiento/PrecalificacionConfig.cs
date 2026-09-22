namespace dashboard.Models.Reclutamiento;

// Una fila por Solicitud — 5 criterios fijos (no extensibles por el usuario), cada uno con peso
// (deben sumar 100 entre todos) y un valor objetivo. Fórmula de puntaje en SolicitudService.CalcularPuntaje.
public class PrecalificacionConfig
{
    public int Id { get; set; }
    public int SolicitudId { get; set; }

    public int SexoPeso { get; set; }
    public string? SexoObjetivo { get; set; }

    public int EdadPeso { get; set; }
    public int EdadMin { get; set; }
    public int EdadMax { get; set; }

    public int ComunaPeso { get; set; }
    public string? ComunaObjetivo { get; set; } // lista separada por comas, ej. "Providencia, Ñuñoa"

    public int ExperienciaPeso { get; set; }
    public int ExperienciaMinimo { get; set; }

    public int RubroPeso { get; set; }
    public string? RubroObjetivo { get; set; }
}
