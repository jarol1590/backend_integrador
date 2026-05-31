using BackendIntegrador.Application.Common;
using BackendIntegrador.Application.Dtos;
using BackendIntegrador.Domain.Entities;

namespace BackendIntegrador.Infrastructure.Services;

internal static class UsuarioPerfilMapper
{
    public static UsuarioPerfilBaseDto Map(
        Usuario usuario,
        Rol rol,
        string tipoUsuario,
        Productor? productorConFincas = null)
    {
        var rolDto = new RolResumenDto(rol.RolId, rol.Nombre, rol.Descripcion);
        var baseArgs = (usuario.UsuarioId, usuario.Email, usuario.Estado, usuario.FechaCreacion, rolDto);

        return tipoUsuario switch
        {
            UsuarioRoleTypes.Administrador => new AdministradorPerfilDto(
                baseArgs.Item1, baseArgs.Item2, baseArgs.Item3, baseArgs.Item4, baseArgs.Item5),

            UsuarioRoleTypes.CentroAcopio => new CentroAcopioPerfilDto(
                baseArgs.Item1, baseArgs.Item2, baseArgs.Item3, baseArgs.Item4, baseArgs.Item5,
                MapCentro(usuario.CentroAcopio!)),

            UsuarioRoleTypes.TrabajadorCentroAcopio => new TrabajadorCentroAcopioPerfilDto(
                baseArgs.Item1, baseArgs.Item2, baseArgs.Item3, baseArgs.Item4, baseArgs.Item5,
                MapCentro(usuario.CentroAcopio!)),

            UsuarioRoleTypes.Productor => new ProductorPerfilDto(
                baseArgs.Item1, baseArgs.Item2, baseArgs.Item3, baseArgs.Item4, baseArgs.Item5,
                MapProductor(productorConFincas ?? usuario.Productor!),
                MapFincas(productorConFincas?.Fincas ?? usuario.Productor?.Fincas)),

            _ => throw new InvalidOperationException($"Tipo de usuario no soportado: {tipoUsuario}")
        };
    }

    private static CentroAcopioResumenDto MapCentro(CentroAcopio centro) =>
        new(centro.CentroAcopioId, centro.Nombre);

    private static ProductorDatosDto MapProductor(Productor productor) =>
        new(productor.ProductorId, productor.Nombre, productor.Documento, productor.Telefono, productor.TipoDocumentoId);

    private static IReadOnlyList<FincaResumenDto> MapFincas(IEnumerable<Finca>? fincas) =>
        fincas?.Select(f => new FincaResumenDto(f.FincaId, f.Nombre, f.MunicipioId)).ToList()
        ?? [];
}
