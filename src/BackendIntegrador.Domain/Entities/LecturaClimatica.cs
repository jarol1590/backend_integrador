namespace BackendIntegrador.Domain.Entities;

public class LecturaClimatica
{
    public int LecturaId { get; set; }
    public int FincaId { get; set; }
    public DateOnly Fecha { get; set; }
    public decimal TempMin { get; set; }
    public decimal TempMax { get; set; }
    public decimal TempMedia { get; set; }
    public decimal? HumedadMedia { get; set; }
    public decimal? PrecipitacionMm { get; set; }
    public decimal? ThiMax { get; set; }
    public int DiasConsecutivosCalor { get; set; }
    public string Fuente { get; set; } = string.Empty;

    public Finca Finca { get; set; } = null!;
}
