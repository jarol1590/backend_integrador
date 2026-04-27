namespace BackendIntegrador.Domain.Entities;

public class Usuario
{
    public int UsuarioId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public int? CentroAcopioId { get; set; }

    public CentroAcopio? CentroAcopio { get; set; }
    public ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
    public Productor? Productor { get; set; }
    public ICollection<RecepcionAcopio> RecepcionesRegistradas { get; set; } = new List<RecepcionAcopio>();
    public ICollection<Muestra> MuestrasComoTecnico { get; set; } = new List<Muestra>();
}
