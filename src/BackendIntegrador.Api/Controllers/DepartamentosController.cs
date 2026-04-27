using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace BackendIntegrador.Api.Controllers;

[ApiController]
[Route("api/departamentos")]
public sealed class DepartamentosController : IntKeyCrudControllerBase<DepartamentoDto, CreateDepartamentoDto, UpdateDepartamentoDto>
{
    public DepartamentosController(ICrudService<DepartamentoDto, CreateDepartamentoDto, UpdateDepartamentoDto> svc)
        : base(svc, d => d.DepartamentoId)
    {
    }
}
