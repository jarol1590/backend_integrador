namespace BackendIntegrador.Domain.Entities;

public class RecepcionAcopio
{
    public int RecepcionId { get; set; }
    public int TransporteId { get; set; }
    public int CentroAcopioId { get; set; }
    public DateTime FechaHoraEntrada { get; set; }
    public int UsuarioId { get; set; }
    public int? TemperaturaRecepcion { get; set; }
    public decimal VolumenLitrosRecibidos { get; set; }

    public Transporte Transporte { get; set; } = null!;
    public CentroAcopio CentroAcopio { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
}
