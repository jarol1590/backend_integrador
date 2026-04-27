using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace BackendIntegrador.Api.Controllers;

[ApiController]
[Route("api/parametros-calidad")]
public sealed class ParametrosCalidadController : IntKeyCrudControllerBase<ParametroCalidadDto, CreateParametroCalidadDto, UpdateParametroCalidadDto>
{
    public ParametrosCalidadController(ICrudService<ParametroCalidadDto, CreateParametroCalidadDto, UpdateParametroCalidadDto> svc)
        : base(svc, p => p.ParametroId)
    {
    }
}
