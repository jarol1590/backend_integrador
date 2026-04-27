using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace BackendIntegrador.Api.Controllers;

[ApiController]
[Route("api/recepciones-acopio")]
public sealed class RecepcionesAcopioController : IntKeyCrudControllerBase<RecepcionAcopioDto, CreateRecepcionAcopioDto, UpdateRecepcionAcopioDto>
{
    public RecepcionesAcopioController(ICrudService<RecepcionAcopioDto, CreateRecepcionAcopioDto, UpdateRecepcionAcopioDto> svc)
        : base(svc, r => r.RecepcionId)
    {
    }
}
