using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace BackendIntegrador.Api.Controllers;

[ApiController]
[Route("api/transportes")]
public sealed class TransportesController : IntKeyCrudControllerBase<TransporteDto, CreateTransporteDto, UpdateTransporteDto>
{
    public TransportesController(ICrudService<TransporteDto, CreateTransporteDto, UpdateTransporteDto> svc)
        : base(svc, t => t.TransporteId)
    {
    }
}
