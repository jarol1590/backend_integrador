using BackendIntegrador.Application.Dtos;
using BackendIntegrador.Domain.Entities;

namespace BackendIntegrador.Infrastructure.Services;

internal static class UsuarioAlcanceHelper
{
    public static string DerivarTipoUsuario(IEnumerable<string> roleNames, int? centroAcopioId)
    {
        var names = roleNames.Select(Normalize).ToList();

        if (names.Any(n => n.Contains("admin")))
            return "admin";
        if (names.Any(n => n.Contains("productor")))
            return "productor";
        if (names.Any(n => n.Contains("tecnico") || n.Contains("técnico")))
            return "tecnico";
        if (names.Any(n => n.Contains("centro") || n.Contains("acopio")) || centroAcopioId.HasValue)
            return "centro_acopio";

        return "sin_asignar";
    }

    public static UsuarioAlcanceDto BuildAlcance(Usuario usuario)
    {
        var roleNames = usuario.UsuarioRoles.Select(ur => ur.Rol.Nombre).ToList();
        var tipo = DerivarTipoUsuario(roleNames, usuario.CentroAcopioId);

        ProductorAlcanceDto? productorDto = null;
        IReadOnlyList<FincaAlcanceDto> fincas;

        if (usuario.Productor is not null)
        {
            productorDto = new ProductorAlcanceDto(
                usuario.Productor.ProductorId,
                usuario.Productor.Nombre,
                usuario.Productor.Documento,
                usuario.Productor.Telefono,
                usuario.Productor.TipoDocumentoId);

            fincas = usuario.Productor.Fincas
                .Select(f => new FincaAlcanceDto(
                    f.FincaId,
                    f.Nombre,
                    f.MunicipioId,
                    PuedeOperarFinca(tipo, f, usuario.CentroAcopio?.MunicipioId)))
                .ToList();
        }
        else if (tipo == "centro_acopio" && usuario.CentroAcopio is not null)
        {
            var municipioCentro = usuario.CentroAcopio.MunicipioId;
            fincas = usuario.CentroAcopio.Municipio?.Fincas
                .Select(f => new FincaAlcanceDto(f.FincaId, f.Nombre, f.MunicipioId, f.MunicipioId == municipioCentro))
                .ToList() ?? [];
        }
        else
        {
            fincas = [];
        }

        return new UsuarioAlcanceDto(tipo, productorDto, fincas);
    }

    private static bool PuedeOperarFinca(string tipo, Finca finca, int? centroMunicipioId) =>
        tipo switch
        {
            "admin" => true,
            "productor" => true,
            "centro_acopio" => centroMunicipioId.HasValue && finca.MunicipioId == centroMunicipioId.Value,
            _ => false
        };

    private static string Normalize(string name) => name.Trim().ToLowerInvariant();
}
