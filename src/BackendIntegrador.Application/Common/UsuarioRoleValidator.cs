using BackendIntegrador.Application.Dtos;
using BackendIntegrador.Domain.Entities;

namespace BackendIntegrador.Application.Common;

public static class UsuarioRoleValidator
{
    public static string ResolveAndValidateRol(Rol rol)
    {
        var tipo = UsuarioRoleTypes.ResolveTipoFromRolNombre(rol.Nombre);
        if (tipo is null)
            throw new InvalidOperationException(
                $"El rol '{rol.Nombre}' no es válido. Roles permitidos: Administrador, Centro de Acopio, Productor, Trabajador Centro de acopio.");
        return tipo;
    }

    public static string ValidateProvision(
        Rol rol,
        int? centroAcopioId,
        ProductorProvisionDto? productor,
        CentroAcopioProvisionDto? centroAcopioProvision = null,
        TrabajadorProvisionDto? trabajadorProvision = null)
    {
        var tipo = ResolveAndValidateRol(rol);
        ValidateStructuralRules(tipo, centroAcopioId, productor, centroAcopioProvision, trabajadorProvision);
        return tipo;
    }

    public static string ValidateUpdate(
        Rol rol,
        int? centroAcopioId,
        ProductorProvisionDto? productor,
        bool hadProductorRecord)
    {
        var tipo = ResolveAndValidateRol(rol);

        if (hadProductorRecord && !UsuarioRoleTypes.IsProductor(tipo))
            throw new InvalidOperationException(
                "No se puede cambiar el rol de un usuario que ya tiene registro de productor. Desactive el usuario y cree uno nuevo.");

        ValidateStructuralRules(tipo, centroAcopioId, productor);
        return tipo;
    }

    private static void ValidateStructuralRules(
        string tipo,
        int? centroAcopioId,
        ProductorProvisionDto? productor,
        CentroAcopioProvisionDto? centroAcopioProvision = null,
        TrabajadorProvisionDto? trabajadorProvision = null)
    {
        switch (tipo)
        {
            case UsuarioRoleTypes.Administrador:
                if (centroAcopioId.HasValue)
                    throw new InvalidOperationException("Un Administrador no puede estar asociado a un Centro de Acopio.");
                if (productor is not null)
                    throw new InvalidOperationException("Un Administrador no puede tener datos de productor ni fincas.");
                break;

            case UsuarioRoleTypes.CentroAcopio:
                if (!centroAcopioId.HasValue && centroAcopioProvision is null)
                    throw new InvalidOperationException("Debe indicar el Centro de Acopio de trabajo.");
                if (productor is not null)
                    throw new InvalidOperationException("Este rol no puede tener datos de productor ni fincas.");
                break;

            case UsuarioRoleTypes.TrabajadorCentroAcopio:
                if (!centroAcopioId.HasValue)
                    throw new InvalidOperationException("Debe indicar el Centro de Acopio de trabajo.");
                if (productor is not null)
                    throw new InvalidOperationException("Este rol no puede tener datos de productor ni fincas.");
                if (trabajadorProvision is null)
                    throw new InvalidOperationException("Los trabajadores requieren datos personales.");
                break;

            case UsuarioRoleTypes.Productor:
                if (centroAcopioId.HasValue)
                    throw new InvalidOperationException("Un Productor no puede estar asociado a un Centro de Acopio.");
                if (productor is null)
                    throw new InvalidOperationException("Los usuarios con rol Productor requieren datos de productor.");
                break;
        }
    }
}
