namespace BackendIntegrador.Domain.Entities;

public class Muestra
{
    public int MuestraId { get; set; }
    public int LoteId { get; set; }
    public int TecnicoPorUsuarioId { get; set; }
    public DateTime FechaHoraToma { get; set; }

    public Lote Lote { get; set; } = null!;
    public Usuario TecnicoUsuario { get; set; } = null!;
    public ICollection<AnalisisCalidad> Analisis { get; set; } = new List<AnalisisCalidad>();
}
