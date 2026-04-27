namespace BackendIntegrador.Domain.Entities;

public class Municipio
{
    public int MunicipioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int DepartamentoId { get; set; }

    public Departamento Departamento { get; set; } = null!;
    public ICollection<CentroAcopio> CentrosAcopio { get; set; } = new List<CentroAcopio>();
    public ICollection<Finca> Fincas { get; set; } = new List<Finca>();
}
