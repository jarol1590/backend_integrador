using System.ComponentModel.DataAnnotations;

namespace BackendIntegrador.Domain.Entities;

public class AnalisisCalidad
{
    [Key]
    public int AnalisisId { get; set; }
    public int MuestraId { get; set; }
    public DateTime FechaHoraAnalisis { get; set; }
    public string? Observaciones { get; set; }

    public Muestra Muestra { get; set; } = null!;
    public ICollection<ResultadoParametro> Resultados { get; set; } = new List<ResultadoParametro>();
}
