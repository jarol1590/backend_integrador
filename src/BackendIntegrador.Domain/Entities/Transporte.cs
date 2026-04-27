namespace BackendIntegrador.Domain.Entities;

public class Transporte
{
    public int TransporteId { get; set; }
    public string PlacaVehiculo { get; set; } = string.Empty;
    public DateTime FechaHoraSalida { get; set; }
    public DateTime? FechaHoraEntrada { get; set; }
    public int? TemperaturaInicio { get; set; }

    public ICollection<Lote> Lotes { get; set; } = new List<Lote>();
    public ICollection<RecepcionAcopio> Recepciones { get; set; } = new List<RecepcionAcopio>();
}
