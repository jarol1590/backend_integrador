using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendIntegrador.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize]
public sealed class UsuariosController : IntKeyCrudControllerBase<UsuarioDto, CreateUsuarioDto, UpdateUsuarioDto>
{
    public UsuariosController(ICrudService<UsuarioDto, CreateUsuarioDto, UpdateUsuarioDto> svc)
        : base(svc, u => u.UsuarioId)
    {
    }

    [AllowAnonymous]
    [HttpPost]
    public override async Task<ActionResult<UsuarioDto>> Create([FromBody] CreateUsuarioDto dto, CancellationToken cancellationToken)
    {
        return await base.Create(dto, cancellationToken);
    }
}
