using Microsoft.AspNetCore.Authorization;

namespace BackendIntegrador.Api.Attributes;

/// <summary>
/// Atributo para proteger endpoints según roles específicos.
/// Se usa junto con [Authorize] para verificar que el usuario tiene los roles requeridos.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeRoleAttribute : AuthorizeAttribute
{
    public AuthorizeRoleAttribute(params string[] roles)
    {
        if (roles.Any())
        {
            Roles = string.Join(",", roles);
        }
    }
}
