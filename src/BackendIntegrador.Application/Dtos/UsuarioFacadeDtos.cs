using System.Text.Json.Serialization;
using BackendIntegrador.Application.Common;

namespace BackendIntegrador.Application.Dtos;

public record CentroAcopioResumenDto(int CentroAcopioId, string Nombre);

public record RolResumenDto(int RolId, string Nombre, string? Descripcion);

public record ProductorDatosDto(
    int ProductorId,
    string Nombre,
    string Documento,
    string? Telefono,
    int TipoDocumentoId);

public record FincaResumenDto(int FincaId, string Nombre, int MunicipioId);

[JsonPolymorphic(TypeDiscriminatorPropertyName = "tipoUsuario")]
[JsonDerivedType(typeof(AdministradorPerfilDto), typeDiscriminator: UsuarioRoleTypes.Administrador)]
[JsonDerivedType(typeof(CentroAcopioPerfilDto), typeDiscriminator: UsuarioRoleTypes.CentroAcopio)]
[JsonDerivedType(typeof(TrabajadorCentroAcopioPerfilDto), typeDiscriminator: UsuarioRoleTypes.TrabajadorCentroAcopio)]
[JsonDerivedType(typeof(ProductorPerfilDto), typeDiscriminator: UsuarioRoleTypes.Productor)]
public abstract record UsuarioPerfilBaseDto(
    int UsuarioId,
    string Email,
    string Estado,
    DateTime FechaCreacion,
    RolResumenDto Rol);

public sealed record AdministradorPerfilDto(
    int UsuarioId,
    string Email,
    string Estado,
    DateTime FechaCreacion,
    RolResumenDto Rol)
    : UsuarioPerfilBaseDto(UsuarioId, Email, Estado, FechaCreacion, Rol);

public sealed record CentroAcopioPerfilDto(
    int UsuarioId,
    string Email,
    string Estado,
    DateTime FechaCreacion,
    RolResumenDto Rol,
    CentroAcopioResumenDto CentroAcopio)
    : UsuarioPerfilBaseDto(UsuarioId, Email, Estado, FechaCreacion, Rol);

public sealed record TrabajadorCentroAcopioPerfilDto(
    int UsuarioId,
    string Email,
    string Estado,
    DateTime FechaCreacion,
    RolResumenDto Rol,
    CentroAcopioResumenDto CentroAcopio)
    : UsuarioPerfilBaseDto(UsuarioId, Email, Estado, FechaCreacion, Rol);

public sealed record ProductorPerfilDto(
    int UsuarioId,
    string Email,
    string Estado,
    DateTime FechaCreacion,
    RolResumenDto Rol,
    ProductorDatosDto Productor,
    IReadOnlyList<FincaResumenDto> Fincas)
    : UsuarioPerfilBaseDto(UsuarioId, Email, Estado, FechaCreacion, Rol);

public record UsuarioListadoDto(
    int UsuarioId,
    string Email,
    string Estado,
    DateTime FechaCreacion,
    string? CentroAcopioNombre,
    string RolNombre,
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
    int RolId,
    int? CentroAcopioId,
    ProductorProvisionDto? Productor);

public record ActualizarUsuarioDto(
    string Email,
    string Estado,
    int RolId,
    int? CentroAcopioId,
    string? Password,
    ProductorProvisionDto? Productor);

public record AuthUsuarioDto(
    int UsuarioId,
    string Email,
    string Estado,
    DateTime FechaCreacion,
    string TipoUsuario,
    string RolNombre);
