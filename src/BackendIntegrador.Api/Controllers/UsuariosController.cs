using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace BackendIntegrador.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
public sealed class UsuariosController : IntKeyCrudControllerBase<UsuarioDto, CreateUsuarioDto, UpdateUsuarioDto>
{
    public UsuariosController(ICrudService<UsuarioDto, CreateUsuarioDto, UpdateUsuarioDto> svc)
        : base(svc, u => u.UsuarioId)
    {
    }
}
