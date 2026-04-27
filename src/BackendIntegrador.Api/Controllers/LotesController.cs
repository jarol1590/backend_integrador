using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace BackendIntegrador.Api.Controllers;

[ApiController]
[Route("api/lotes")]
public sealed class LotesController : IntKeyCrudControllerBase<LoteDto, CreateLoteDto, UpdateLoteDto>
{
    public LotesController(ICrudService<LoteDto, CreateLoteDto, UpdateLoteDto> svc)
        : base(svc, l => l.LoteId)
    {
    }
}
