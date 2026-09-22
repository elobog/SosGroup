namespace dashboard.Models.Reclutamiento;

public class Postulante
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string RUT { get; set; } = string.Empty;
    public string? Correo { get; set; }
    public string? Telefono { get; set; }
    public string? Sexo { get; set; }
    public int? Edad { get; set; }
    public string? Comuna { get; set; }
    public int? AniosExperiencia { get; set; }
    public string? RubroExperiencia { get; set; }
    public string? EstadoCivil { get; set; }
    public string? Nacionalidad { get; set; }
    public bool? Discapacidad { get; set; }
    public decimal? RentaPretendida { get; set; }
    public string? TituloCV { get; set; }
    public string? DescripcionProfesional { get; set; }
    public string? Habilidades { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Portal público de postulación — ver PostulanteAccesoToken.
    public bool PoliticaAceptada { get; set; }
    public DateTime? FechaAceptacionPolitica { get; set; }
}
