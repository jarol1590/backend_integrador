namespace BackendIntegrador.Domain.Entities;

public class Productor
{
    public int ProductorId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public int UsuarioId { get; set; }
    public int TipoDocumentoId { get; set; }

    public Usuario Usuario { get; set; } = null!;
    public TipoDocumento TipoDocumento { get; set; } = null!;
    public ICollection<Finca> Fincas { get; set; } = new List<Finca>();
}
