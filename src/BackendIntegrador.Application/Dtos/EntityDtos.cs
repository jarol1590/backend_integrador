namespace BackendIntegrador.Application.Dtos;

public record UsuarioDto(int UsuarioId, string Email, string Estado, DateTime FechaCreacion, int? CentroAcopioId);
public record CreateUsuarioDto(string Email, string Password, string Estado, int? CentroAcopioId, DateTime? FechaCreacion);
public record UpdateUsuarioDto(string Email, string Estado, int? CentroAcopioId, string? Password);

public record RolDto(int RolId, string Nombre, string? Descripcion);
public record CreateRolDto(string Nombre, string? Descripcion);
public record UpdateRolDto(string Nombre, string? Descripcion);

public record UsuarioRolDto(int UsuarioId, int RolId);
public record CreateUsuarioRolDto(int UsuarioId, int RolId);

public record DepartamentoDto(int DepartamentoId, string Nombre);
public record CreateDepartamentoDto(string Nombre);
public record UpdateDepartamentoDto(string Nombre);

public record MunicipioDto(int MunicipioId, string Nombre, int DepartamentoId);
public record CreateMunicipioDto(string Nombre, int DepartamentoId);
public record UpdateMunicipioDto(string Nombre, int DepartamentoId);

public record CentroAcopioDto(int CentroAcopioId, string Nombre, string? Direccion, decimal? Latitud, decimal? Longitud, int MunicipioId);
public record CreateCentroAcopioDto(string Nombre, string? Direccion, decimal? Latitud, decimal? Longitud, int MunicipioId);
public record UpdateCentroAcopioDto(string Nombre, string? Direccion, decimal? Latitud, decimal? Longitud, int MunicipioId);

public record TipoDocumentoDto(int TipoDocumentoId, string Nombre, string? Descripcion);
public record CreateTipoDocumentoDto(string Nombre, string? Descripcion);
public record UpdateTipoDocumentoDto(string Nombre, string? Descripcion);

public record ProductorDto(int ProductorId, string Nombre, string Documento, string? Telefono, int UsuarioId, int TipoDocumentoId);
public record CreateProductorDto(string Nombre, string Documento, string? Telefono, int UsuarioId, int TipoDocumentoId);
public record UpdateProductorDto(string Nombre, string Documento, string? Telefono, int UsuarioId, int TipoDocumentoId);

public record FincaDto(int FincaId, string Nombre, string? Direccion, decimal? Latitud, decimal? Longitud, int ProductorId, int MunicipioId);
public record CreateFincaDto(string Nombre, string? Direccion, decimal? Latitud, decimal? Longitud, int ProductorId, int MunicipioId);
public record UpdateFincaDto(string Nombre, string? Direccion, decimal? Latitud, decimal? Longitud, int ProductorId, int MunicipioId);

public record OrdenoDto(int OrdenoId, DateTime FechaHoraInicio, DateTime? FechaHoraFin, decimal VolumenLitros, int FincaId);
public record CreateOrdenoDto(DateTime FechaHoraInicio, DateTime? FechaHoraFin, decimal VolumenLitros, int FincaId);
public record UpdateOrdenoDto(DateTime FechaHoraInicio, DateTime? FechaHoraFin, decimal VolumenLitros, int FincaId);

public record TransporteDto(int TransporteId, string PlacaVehiculo, DateTime FechaHoraSalida, DateTime? FechaHoraEntrada, int? TemperaturaInicio);
public record CreateTransporteDto(string PlacaVehiculo, DateTime FechaHoraSalida, DateTime? FechaHoraEntrada, int? TemperaturaInicio);
public record UpdateTransporteDto(string PlacaVehiculo, DateTime FechaHoraSalida, DateTime? FechaHoraEntrada, int? TemperaturaInicio);

public record LoteDto(int LoteId, int OrdenoId, int CentroAcopioId, decimal VolumenCapturadoLitros, int? TransporteId);
public record CreateLoteDto(int OrdenoId, int CentroAcopioId, decimal VolumenCapturadoLitros, int? TransporteId);
public record UpdateLoteDto(int OrdenoId, int CentroAcopioId, decimal VolumenCapturadoLitros, int? TransporteId);

public record RecepcionAcopioDto(int RecepcionId, int TransporteId, int CentroAcopioId, DateTime FechaHoraEntrada, int UsuarioId, int? TemperaturaRecepcion, decimal VolumenLitrosRecibidos);
public record CreateRecepcionAcopioDto(int TransporteId, int CentroAcopioId, DateTime FechaHoraEntrada, int UsuarioId, int? TemperaturaRecepcion, decimal VolumenLitrosRecibidos);
public record UpdateRecepcionAcopioDto(int TransporteId, int CentroAcopioId, DateTime FechaHoraEntrada, int UsuarioId, int? TemperaturaRecepcion, decimal VolumenLitrosRecibidos);

public record MuestraDto(int MuestraId, int LoteId, int TecnicoPorUsuarioId, DateTime FechaHoraToma);
public record CreateMuestraDto(int LoteId, int TecnicoPorUsuarioId, DateTime FechaHoraToma);
public record UpdateMuestraDto(int LoteId, int TecnicoPorUsuarioId, DateTime FechaHoraToma);

public record AnalisisCalidadDto(int AnalisisId, int MuestraId, DateTime FechaHoraAnalisis, string? Observaciones);
public record CreateAnalisisCalidadDto(int MuestraId, DateTime FechaHoraAnalisis, string? Observaciones);
public record UpdateAnalisisCalidadDto(int MuestraId, DateTime FechaHoraAnalisis, string? Observaciones);

public record ParametroCalidadDto(int ParametroId, string Nombre, string? Unidad, decimal? ValorMinimo, decimal? ValorMaximo);
public record CreateParametroCalidadDto(string Nombre, string? Unidad, decimal? ValorMinimo, decimal? ValorMaximo);
public record UpdateParametroCalidadDto(string Nombre, string? Unidad, decimal? ValorMinimo, decimal? ValorMaximo);

public record ResultadoParametroDto(int AnalisisId, int ParametroId, decimal ValorResultado, string? Observacion);
public record CreateResultadoParametroDto(int AnalisisId, int ParametroId, decimal ValorResultado, string? Observacion);
public record UpdateResultadoParametroDto(decimal ValorResultado, string? Observacion);
