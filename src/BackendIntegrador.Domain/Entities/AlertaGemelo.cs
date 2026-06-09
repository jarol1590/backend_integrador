namespace BackendIntegrador.Domain.Entities;

public class AlertaGemelo
{
    public int AlertaId { get; set; }
    public int FincaId { get; set; }
    public string TipoAlerta { get; set; } = string.Empty;
    public string Severidad { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string? Recomendacion { get; set; }
    public DateTime CreadaUtc { get; set; }
    public DateTime? ExpiraUtc { get; set; }
    public bool Leida { get; set; }
    public DateTime? LeidaUtc { get; set; }

    public Finca Finca { get; set; } = null!;
}
