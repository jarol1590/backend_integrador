namespace BackendIntegrador.Domain.Entities;

public class TipoDocumento
{
    public int TipoDocumentoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    public ICollection<Productor> Productores { get; set; } = new List<Productor>();
}
