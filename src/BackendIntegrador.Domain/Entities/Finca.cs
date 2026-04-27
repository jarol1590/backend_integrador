namespace BackendIntegrador.Domain.Entities;

public class Finca
{
    public int FincaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }
    public int ProductorId { get; set; }
    public int MunicipioId { get; set; }

    public Productor Productor { get; set; } = null!;
    public Municipio Municipio { get; set; } = null!;
    public ICollection<Ordeno> Ordenos { get; set; } = new List<Ordeno>();
}
