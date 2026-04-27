namespace BackendIntegrador.Domain.Entities;

public class Lote
{
    public int LoteId { get; set; }
    public int OrdenoId { get; set; }
    public int CentroAcopioId { get; set; }
    public decimal VolumenCapturadoLitros { get; set; }
    public int? TransporteId { get; set; }

    public Ordeno Ordeno { get; set; } = null!;
    public CentroAcopio CentroAcopio { get; set; } = null!;
    public Transporte? Transporte { get; set; }
    public ICollection<Muestra> Muestras { get; set; } = new List<Muestra>();
}
