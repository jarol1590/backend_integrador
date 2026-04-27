using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace BackendIntegrador.Api.Controllers;

[ApiController]
[Route("api/centros-acopio")]
public sealed class CentrosAcopioController : IntKeyCrudControllerBase<CentroAcopioDto, CreateCentroAcopioDto, UpdateCentroAcopioDto>
{
    public CentrosAcopioController(ICrudService<CentroAcopioDto, CreateCentroAcopioDto, UpdateCentroAcopioDto> svc)
        : base(svc, c => c.CentroAcopioId)
    {
    }
}
