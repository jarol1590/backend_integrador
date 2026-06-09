namespace BackendIntegrador.Domain.Entities;

public class PrediccionGemelo
{
    public int PrediccionId { get; set; }
    public int FincaId { get; set; }
    public DateTime GeneradaUtc { get; set; }
    public int HorizonteDias { get; set; }
    public string TipoPrediccion { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public decimal Confianza { get; set; }
    public string? Unidad { get; set; }
    public string? DetalleJson { get; set; }

    public Finca Finca { get; set; } = null!;
}
