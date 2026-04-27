namespace BackendIntegrador.Domain.Entities;

public class Ordeno
{
    public int OrdenoId { get; set; }
    public DateTime FechaHoraInicio { get; set; }
    public DateTime? FechaHoraFin { get; set; }
    public decimal VolumenLitros { get; set; }
    public int FincaId { get; set; }

    public Finca Finca { get; set; } = null!;
    public ICollection<Lote> Lotes { get; set; } = new List<Lote>();
}
