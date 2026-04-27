using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace BackendIntegrador.Api.Controllers;

[ApiController]
[Route("api/analisis-calidad")]
public sealed class AnalisisCalidadController : IntKeyCrudControllerBase<AnalisisCalidadDto, CreateAnalisisCalidadDto, UpdateAnalisisCalidadDto>
{
    public AnalisisCalidadController(ICrudService<AnalisisCalidadDto, CreateAnalisisCalidadDto, UpdateAnalisisCalidadDto> svc)
        : base(svc, a => a.AnalisisId)
    {
    }
}
