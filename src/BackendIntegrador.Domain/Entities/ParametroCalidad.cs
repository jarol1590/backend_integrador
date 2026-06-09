using System.ComponentModel.DataAnnotations;

namespace BackendIntegrador.Domain.Entities;

public class ParametroCalidad
{
    [Key]
    public int ParametroId { get; set; }
    public int? CentroAcopioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Unidad { get; set; }
    public decimal? ValorMinimo { get; set; }
    public decimal? ValorMaximo { get; set; }
    public string? Descripcion { get; set; }
    public int Orden { get; set; }

    public CentroAcopio? CentroAcopio { get; set; }
    public ICollection<ResultadoParametro> Resultados { get; set; } = new List<ResultadoParametro>();
}
