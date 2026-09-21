namespace dashboard.Models.Reclutamiento;

public class PerfilCargoVersion
{
    public int Id { get; set; }
    public int PerfilCargoId { get; set; }
    public int NumeroVersion { get; set; }
    public string? RentaFija { get; set; }
    public string? RentaVariable { get; set; }
    public string? Beneficios { get; set; }
    public DateTime FechaVigencia { get; set; }
    public string Estado { get; set; } = "Pendiente"; // Pendiente / Vigente / Historica
    public int? VersionBaseId { get; set; }

    // Formato estándar SOS Group — se completa a mano o se precompleta analizando el documento
    // del cliente con IA (ver PerfilCargoDocumentoOriginal).
    public string? Area { get; set; }
    public string? ReportaA { get; set; }
    public string? ObjetivoCargo { get; set; }
    public string? EducacionMinima { get; set; }
    public int? AniosExperienciaMinimo { get; set; }
    public string? ConocimientosTecnicos { get; set; }
    public string? Habilidades { get; set; }
    public string? CondicionesEspeciales { get; set; }
    public string OrigenDocumento { get; set; } = "Manual"; // Manual / AnalizadoIA
}
