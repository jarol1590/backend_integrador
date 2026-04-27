using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace BackendIntegrador.Api.Controllers;

[ApiController]
[Route("api/ordenos")]
public sealed class OrdenosController : IntKeyCrudControllerBase<OrdenoDto, CreateOrdenoDto, UpdateOrdenoDto>
{
    public OrdenosController(ICrudService<OrdenoDto, CreateOrdenoDto, UpdateOrdenoDto> svc)
        : base(svc, o => o.OrdenoId)
    {
    }
}
