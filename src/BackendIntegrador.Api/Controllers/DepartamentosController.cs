using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
namespace BackendIntegrador.Api.Controllers;

[ApiController]
[Route("api/departamentos")]
[AllowAnonymous]
public sealed class DepartamentosController : IntKeyCrudControllerBase<DepartamentoDto, CreateDepartamentoDto, UpdateDepartamentoDto>
{
    public DepartamentosController(ICrudService<DepartamentoDto, CreateDepartamentoDto, UpdateDepartamentoDto> svc)
        : base(svc, d => d.DepartamentoId)
    {
    }
}
