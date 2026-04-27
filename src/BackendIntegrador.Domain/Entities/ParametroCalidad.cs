namespace BackendIntegrador.Domain.Entities;

public class ParametroCalidad
{
    public int ParametroId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Unidad { get; set; }
    public decimal? ValorMinimo { get; set; }
    public decimal? ValorMaximo { get; set; }

    public ICollection<ResultadoParametro> Resultados { get; set; } = new List<ResultadoParametro>();
}
