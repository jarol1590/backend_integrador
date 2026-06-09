namespace BackendIntegrador.Domain.Entities;

public class FincaGemeloEstado
{
    public int FincaId { get; set; }
    public DateTime? UltimaSyncUtc { get; set; }
    public string VersionMotor { get; set; } = string.Empty;
    public string FuenteClima { get; set; } = string.Empty;
    public int ScoreRiesgoGlobal { get; set; }
    public string EstadoSync { get; set; } = "pendiente";
    public string? UltimoError { get; set; }
    public DateTime CreadoUtc { get; set; }
    public DateTime ActualizadoUtc { get; set; }

    public Finca Finca { get; set; } = null!;
}
