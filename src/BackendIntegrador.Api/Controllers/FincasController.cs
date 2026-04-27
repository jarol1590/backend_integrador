using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace BackendIntegrador.Api.Controllers;

[ApiController]
[Route("api/fincas")]
public sealed class FincasController : IntKeyCrudControllerBase<FincaDto, CreateFincaDto, UpdateFincaDto>
{
    public FincasController(ICrudService<FincaDto, CreateFincaDto, UpdateFincaDto> svc)
        : base(svc, f => f.FincaId)
    {
    }
}
