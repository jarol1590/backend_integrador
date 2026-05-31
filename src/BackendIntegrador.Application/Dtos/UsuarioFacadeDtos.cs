namespace BackendIntegrador.Application.Dtos;

public record CentroAcopioResumenDto(int CentroAcopioId, string Nombre);

public record RolResumenDto(int RolId, string Nombre, string? Descripcion);

public record ProductorAlcanceDto(
    int ProductorId,
    string Nombre,
    string Documento,
    string? Telefono,
    int TipoDocumentoId);

public record FincaAlcanceDto(int FincaId, string Nombre, int MunicipioId, bool PuedeOperar);

public record UsuarioAlcanceDto(
    string Tipo,
    ProductorAlcanceDto? Productor,
    IReadOnlyList<FincaAlcanceDto> Fincas);

public record UsuarioPerfilDto(
    int UsuarioId,
    string Email,
    string Estado,
    DateTime FechaCreacion,
    CentroAcopioResumenDto? CentroAcopio,
    IReadOnlyList<RolResumenDto> Roles,
    UsuarioAlcanceDto Alcance);

public record UsuarioListadoDto(
    int UsuarioId,
    string Email,
    string Estado,
    DateTime FechaCreacion,
    string? CentroAcopioNombre,
    IReadOnlyList<string> RolesResumen,
    string TipoUsuario);

public record FincaInicialDto(
    string Nombre,
    string? Direccion,
    decimal? Latitud,
    decimal? Longitud,
    int MunicipioId);

public record ProductorProvisionDto(
    string Nombre,
    string Documento,
    string? Telefono,
    int TipoDocumentoId,
    FincaInicialDto? FincaInicial);

public record ProvisionarUsuarioDto(
    string Email,
    string Password,
    string Estado,
    int? CentroAcopioId,
    IReadOnlyList<int> RolIds,
    ProductorProvisionDto? Productor);

public record ActualizarUsuarioDto(
    string Email,
    string Estado,
    int? CentroAcopioId,
    string? Password,
    IReadOnlyList<int> RolIds,
    ProductorProvisionDto? Productor);

public record AuthUsuarioDto(
    int UsuarioId,
    string Email,
    string Estado,
    DateTime FechaCreacion,
    int? CentroAcopioId,
    IReadOnlyList<string> Roles,
    string AlcanceResumido);
