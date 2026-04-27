namespace BackendIntegrador.Domain.Entities;

public class ResultadoParametro
{
    public int AnalisisId { get; set; }
    public int ParametroId { get; set; }
    public decimal ValorResultado { get; set; }
    public string? Observacion { get; set; }

    public AnalisisCalidad Analisis { get; set; } = null!;
    public ParametroCalidad Parametro { get; set; } = null!;
}
