namespace BackendIntegrador.Domain.Entities;

public class CentroAcopio
{
    public int CentroAcopioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }
    public int MunicipioId { get; set; }

    public Municipio Municipio { get; set; } = null!;
    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    public ICollection<Lote> Lotes { get; set; } = new List<Lote>();
    public ICollection<RecepcionAcopio> Recepciones { get; set; } = new List<RecepcionAcopio>();
}
