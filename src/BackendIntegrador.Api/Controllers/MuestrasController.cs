using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace BackendIntegrador.Api.Controllers;

[ApiController]
[Route("api/muestras")]
public sealed class MuestrasController : IntKeyCrudControllerBase<MuestraDto, CreateMuestraDto, UpdateMuestraDto>
{
    public MuestrasController(ICrudService<MuestraDto, CreateMuestraDto, UpdateMuestraDto> svc)
        : base(svc, m => m.MuestraId)
    {
    }
}
