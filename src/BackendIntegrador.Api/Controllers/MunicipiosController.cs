using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace BackendIntegrador.Api.Controllers;

[ApiController]
[Route("api/municipios")]
public sealed class MunicipiosController : IntKeyCrudControllerBase<MunicipioDto, CreateMunicipioDto, UpdateMunicipioDto>
{
    public MunicipiosController(ICrudService<MunicipioDto, CreateMunicipioDto, UpdateMunicipioDto> svc)
        : base(svc, m => m.MunicipioId)
    {
    }
}
