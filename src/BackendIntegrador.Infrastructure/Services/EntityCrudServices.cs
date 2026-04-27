using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using BackendIntegrador.Domain.Entities;

namespace BackendIntegrador.Infrastructure.Services;

internal sealed class RolCrudService : IntKeyCrudServiceBase<Rol, RolDto, CreateRolDto, UpdateRolDto>
{
    public RolCrudService(IRepository<Rol> repo) : base(repo) { }
    protected override int GetId(Rol e) => e.RolId;
    protected override RolDto MapRead(Rol e) => new(e.RolId, e.Nombre, e.Descripcion);
    protected override Rol MapCreate(CreateRolDto d) => new() { Nombre = d.Nombre, Descripcion = d.Descripcion };
    protected override void ApplyUpdate(Rol e, UpdateRolDto d)
    {
        e.Nombre = d.Nombre;
        e.Descripcion = d.Descripcion;
    }
}

internal sealed class DepartamentoCrudService : IntKeyCrudServiceBase<Departamento, DepartamentoDto, CreateDepartamentoDto, UpdateDepartamentoDto>
{
    public DepartamentoCrudService(IRepository<Departamento> repo) : base(repo) { }
    protected override int GetId(Departamento e) => e.DepartamentoId;
    protected override DepartamentoDto MapRead(Departamento e) => new(e.DepartamentoId, e.Nombre);
    protected override Departamento MapCreate(CreateDepartamentoDto d) => new() { Nombre = d.Nombre };
    protected override void ApplyUpdate(Departamento e, UpdateDepartamentoDto d) => e.Nombre = d.Nombre;
}

internal sealed class MunicipioCrudService : IntKeyCrudServiceBase<Municipio, MunicipioDto, CreateMunicipioDto, UpdateMunicipioDto>
{
    public MunicipioCrudService(IRepository<Municipio> repo) : base(repo) { }
    protected override int GetId(Municipio e) => e.MunicipioId;
    protected override MunicipioDto MapRead(Municipio e) => new(e.MunicipioId, e.Nombre, e.DepartamentoId);
    protected override Municipio MapCreate(CreateMunicipioDto d) => new() { Nombre = d.Nombre, DepartamentoId = d.DepartamentoId };
    protected override void ApplyUpdate(Municipio e, UpdateMunicipioDto d)
    {
        e.Nombre = d.Nombre;
        e.DepartamentoId = d.DepartamentoId;
    }
}

internal sealed class CentroAcopioCrudService : IntKeyCrudServiceBase<CentroAcopio, CentroAcopioDto, CreateCentroAcopioDto, UpdateCentroAcopioDto>
{
    public CentroAcopioCrudService(IRepository<CentroAcopio> repo) : base(repo) { }
    protected override int GetId(CentroAcopio e) => e.CentroAcopioId;
    protected override CentroAcopioDto MapRead(CentroAcopio e) => new(e.CentroAcopioId, e.Nombre, e.Direccion, e.Latitud, e.Longitud, e.MunicipioId);
    protected override CentroAcopio MapCreate(CreateCentroAcopioDto d) => new()
    {
        Nombre = d.Nombre,
        Direccion = d.Direccion,
        Latitud = d.Latitud,
        Longitud = d.Longitud,
        MunicipioId = d.MunicipioId,
    };
    protected override void ApplyUpdate(CentroAcopio e, UpdateCentroAcopioDto d)
    {
        e.Nombre = d.Nombre;
        e.Direccion = d.Direccion;
        e.Latitud = d.Latitud;
        e.Longitud = d.Longitud;
        e.MunicipioId = d.MunicipioId;
    }
}

internal sealed class TipoDocumentoCrudService : IntKeyCrudServiceBase<TipoDocumento, TipoDocumentoDto, CreateTipoDocumentoDto, UpdateTipoDocumentoDto>
{
    public TipoDocumentoCrudService(IRepository<TipoDocumento> repo) : base(repo) { }
    protected override int GetId(TipoDocumento e) => e.TipoDocumentoId;
    protected override TipoDocumentoDto MapRead(TipoDocumento e) => new(e.TipoDocumentoId, e.Nombre, e.Descripcion);
    protected override TipoDocumento MapCreate(CreateTipoDocumentoDto d) => new() { Nombre = d.Nombre, Descripcion = d.Descripcion };
    protected override void ApplyUpdate(TipoDocumento e, UpdateTipoDocumentoDto d)
    {
        e.Nombre = d.Nombre;
        e.Descripcion = d.Descripcion;
    }
}

internal sealed class ProductorCrudService : IntKeyCrudServiceBase<Productor, ProductorDto, CreateProductorDto, UpdateProductorDto>
{
    public ProductorCrudService(IRepository<Productor> repo) : base(repo) { }
    protected override int GetId(Productor e) => e.ProductorId;
    protected override ProductorDto MapRead(Productor e) => new(e.ProductorId, e.Nombre, e.Documento, e.Telefono, e.UsuarioId, e.TipoDocumentoId);
    protected override Productor MapCreate(CreateProductorDto d) => new()
    {
        Nombre = d.Nombre,
        Documento = d.Documento,
        Telefono = d.Telefono,
        UsuarioId = d.UsuarioId,
        TipoDocumentoId = d.TipoDocumentoId,
    };
    protected override void ApplyUpdate(Productor e, UpdateProductorDto d)
    {
        e.Nombre = d.Nombre;
        e.Documento = d.Documento;
        e.Telefono = d.Telefono;
        e.UsuarioId = d.UsuarioId;
        e.TipoDocumentoId = d.TipoDocumentoId;
    }
}

internal sealed class FincaCrudService : IntKeyCrudServiceBase<Finca, FincaDto, CreateFincaDto, UpdateFincaDto>
{
    public FincaCrudService(IRepository<Finca> repo) : base(repo) { }
    protected override int GetId(Finca e) => e.FincaId;
    protected override FincaDto MapRead(Finca e) => new(e.FincaId, e.Nombre, e.Direccion, e.Latitud, e.Longitud, e.ProductorId, e.MunicipioId);
    protected override Finca MapCreate(CreateFincaDto d) => new()
    {
        Nombre = d.Nombre,
        Direccion = d.Direccion,
        Latitud = d.Latitud,
        Longitud = d.Longitud,
        ProductorId = d.ProductorId,
        MunicipioId = d.MunicipioId,
    };
    protected override void ApplyUpdate(Finca e, UpdateFincaDto d)
    {
        e.Nombre = d.Nombre;
        e.Direccion = d.Direccion;
        e.Latitud = d.Latitud;
        e.Longitud = d.Longitud;
        e.ProductorId = d.ProductorId;
        e.MunicipioId = d.MunicipioId;
    }
}

internal sealed class OrdenoCrudService : IntKeyCrudServiceBase<Ordeno, OrdenoDto, CreateOrdenoDto, UpdateOrdenoDto>
{
    public OrdenoCrudService(IRepository<Ordeno> repo) : base(repo) { }
    protected override int GetId(Ordeno e) => e.OrdenoId;
    protected override OrdenoDto MapRead(Ordeno e) => new(e.OrdenoId, e.FechaHoraInicio, e.FechaHoraFin, e.VolumenLitros, e.FincaId);
    protected override Ordeno MapCreate(CreateOrdenoDto d) => new()
    {
        FechaHoraInicio = d.FechaHoraInicio,
        FechaHoraFin = d.FechaHoraFin,
        VolumenLitros = d.VolumenLitros,
        FincaId = d.FincaId,
    };
    protected override void ApplyUpdate(Ordeno e, UpdateOrdenoDto d)
    {
        e.FechaHoraInicio = d.FechaHoraInicio;
        e.FechaHoraFin = d.FechaHoraFin;
        e.VolumenLitros = d.VolumenLitros;
        e.FincaId = d.FincaId;
    }
}

internal sealed class TransporteCrudService : IntKeyCrudServiceBase<Transporte, TransporteDto, CreateTransporteDto, UpdateTransporteDto>
{
    public TransporteCrudService(IRepository<Transporte> repo) : base(repo) { }
    protected override int GetId(Transporte e) => e.TransporteId;
    protected override TransporteDto MapRead(Transporte e) => new(e.TransporteId, e.PlacaVehiculo, e.FechaHoraSalida, e.FechaHoraEntrada, e.TemperaturaInicio);
    protected override Transporte MapCreate(CreateTransporteDto d) => new()
    {
        PlacaVehiculo = d.PlacaVehiculo,
        FechaHoraSalida = d.FechaHoraSalida,
        FechaHoraEntrada = d.FechaHoraEntrada,
        TemperaturaInicio = d.TemperaturaInicio,
    };
    protected override void ApplyUpdate(Transporte e, UpdateTransporteDto d)
    {
        e.PlacaVehiculo = d.PlacaVehiculo;
        e.FechaHoraSalida = d.FechaHoraSalida;
        e.FechaHoraEntrada = d.FechaHoraEntrada;
        e.TemperaturaInicio = d.TemperaturaInicio;
    }
}

internal sealed class LoteCrudService : IntKeyCrudServiceBase<Lote, LoteDto, CreateLoteDto, UpdateLoteDto>
{
    public LoteCrudService(IRepository<Lote> repo) : base(repo) { }
    protected override int GetId(Lote e) => e.LoteId;
    protected override LoteDto MapRead(Lote e) => new(e.LoteId, e.OrdenoId, e.CentroAcopioId, e.VolumenCapturadoLitros, e.TransporteId);
    protected override Lote MapCreate(CreateLoteDto d) => new()
    {
        OrdenoId = d.OrdenoId,
        CentroAcopioId = d.CentroAcopioId,
        VolumenCapturadoLitros = d.VolumenCapturadoLitros,
        TransporteId = d.TransporteId,
    };
    protected override void ApplyUpdate(Lote e, UpdateLoteDto d)
    {
        e.OrdenoId = d.OrdenoId;
        e.CentroAcopioId = d.CentroAcopioId;
        e.VolumenCapturadoLitros = d.VolumenCapturadoLitros;
        e.TransporteId = d.TransporteId;
    }
}

internal sealed class RecepcionAcopioCrudService : IntKeyCrudServiceBase<RecepcionAcopio, RecepcionAcopioDto, CreateRecepcionAcopioDto, UpdateRecepcionAcopioDto>
{
    public RecepcionAcopioCrudService(IRepository<RecepcionAcopio> repo) : base(repo) { }
    protected override int GetId(RecepcionAcopio e) => e.RecepcionId;
    protected override RecepcionAcopioDto MapRead(RecepcionAcopio e) => new(e.RecepcionId, e.TransporteId, e.CentroAcopioId, e.FechaHoraEntrada, e.UsuarioId, e.TemperaturaRecepcion, e.VolumenLitrosRecibidos);
    protected override RecepcionAcopio MapCreate(CreateRecepcionAcopioDto d) => new()
    {
        TransporteId = d.TransporteId,
        CentroAcopioId = d.CentroAcopioId,
        FechaHoraEntrada = d.FechaHoraEntrada,
        UsuarioId = d.UsuarioId,
        TemperaturaRecepcion = d.TemperaturaRecepcion,
        VolumenLitrosRecibidos = d.VolumenLitrosRecibidos,
    };
    protected override void ApplyUpdate(RecepcionAcopio e, UpdateRecepcionAcopioDto d)
    {
        e.TransporteId = d.TransporteId;
        e.CentroAcopioId = d.CentroAcopioId;
        e.FechaHoraEntrada = d.FechaHoraEntrada;
        e.UsuarioId = d.UsuarioId;
        e.TemperaturaRecepcion = d.TemperaturaRecepcion;
        e.VolumenLitrosRecibidos = d.VolumenLitrosRecibidos;
    }
}

internal sealed class MuestraCrudService : IntKeyCrudServiceBase<Muestra, MuestraDto, CreateMuestraDto, UpdateMuestraDto>
{
    public MuestraCrudService(IRepository<Muestra> repo) : base(repo) { }
    protected override int GetId(Muestra e) => e.MuestraId;
    protected override MuestraDto MapRead(Muestra e) => new(e.MuestraId, e.LoteId, e.TecnicoPorUsuarioId, e.FechaHoraToma);
    protected override Muestra MapCreate(CreateMuestraDto d) => new()
    {
        LoteId = d.LoteId,
        TecnicoPorUsuarioId = d.TecnicoPorUsuarioId,
        FechaHoraToma = d.FechaHoraToma,
    };
    protected override void ApplyUpdate(Muestra e, UpdateMuestraDto d)
    {
        e.LoteId = d.LoteId;
        e.TecnicoPorUsuarioId = d.TecnicoPorUsuarioId;
        e.FechaHoraToma = d.FechaHoraToma;
    }
}

internal sealed class AnalisisCalidadCrudService : IntKeyCrudServiceBase<AnalisisCalidad, AnalisisCalidadDto, CreateAnalisisCalidadDto, UpdateAnalisisCalidadDto>
{
    public AnalisisCalidadCrudService(IRepository<AnalisisCalidad> repo) : base(repo) { }
    protected override int GetId(AnalisisCalidad e) => e.AnalisisId;
    protected override AnalisisCalidadDto MapRead(AnalisisCalidad e) => new(e.AnalisisId, e.MuestraId, e.FechaHoraAnalisis, e.Observaciones);
    protected override AnalisisCalidad MapCreate(CreateAnalisisCalidadDto d) => new()
    {
        MuestraId = d.MuestraId,
        FechaHoraAnalisis = d.FechaHoraAnalisis,
        Observaciones = d.Observaciones,
    };
    protected override void ApplyUpdate(AnalisisCalidad e, UpdateAnalisisCalidadDto d)
    {
        e.MuestraId = d.MuestraId;
        e.FechaHoraAnalisis = d.FechaHoraAnalisis;
        e.Observaciones = d.Observaciones;
    }
}

internal sealed class ParametroCalidadCrudService : IntKeyCrudServiceBase<ParametroCalidad, ParametroCalidadDto, CreateParametroCalidadDto, UpdateParametroCalidadDto>
{
    public ParametroCalidadCrudService(IRepository<ParametroCalidad> repo) : base(repo) { }
    protected override int GetId(ParametroCalidad e) => e.ParametroId;
    protected override ParametroCalidadDto MapRead(ParametroCalidad e) => new(e.ParametroId, e.Nombre, e.Unidad, e.ValorMinimo, e.ValorMaximo);
    protected override ParametroCalidad MapCreate(CreateParametroCalidadDto d) => new()
    {
        Nombre = d.Nombre,
        Unidad = d.Unidad,
        ValorMinimo = d.ValorMinimo,
        ValorMaximo = d.ValorMaximo,
    };
    protected override void ApplyUpdate(ParametroCalidad e, UpdateParametroCalidadDto d)
    {
        e.Nombre = d.Nombre;
        e.Unidad = d.Unidad;
        e.ValorMinimo = d.ValorMinimo;
        e.ValorMaximo = d.ValorMaximo;
    }
}
