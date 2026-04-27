using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace BackendIntegrador.Api.Controllers;

[ApiController]
[Route("api/roles")]
public sealed class RolesController : IntKeyCrudControllerBase<RolDto, CreateRolDto, UpdateRolDto>
{
    public RolesController(ICrudService<RolDto, CreateRolDto, UpdateRolDto> svc)
        : base(svc, r => r.RolId)
    {
    }
}
