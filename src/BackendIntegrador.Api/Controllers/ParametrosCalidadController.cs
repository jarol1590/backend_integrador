using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendIntegrador.Api.Controllers;

[ApiController]
[Route("api/parametros-calidad")]
public sealed class ParametrosCalidadController : IntKeyCrudControllerBase<ParametroCalidadDto, CreateParametroCalidadDto, UpdateParametroCalidadDto>
{
    private readonly ICrudService<ParametroCalidadDto, CreateParametroCalidadDto, UpdateParametroCalidadDto> _svc;
    private readonly BackendIntegrador.Infrastructure.Persistence.AppDbContext _db;

    public ParametrosCalidadController(
        ICrudService<ParametroCalidadDto, CreateParametroCalidadDto, UpdateParametroCalidadDto> svc,
        BackendIntegrador.Infrastructure.Persistence.AppDbContext db)
        : base(svc, p => p.ParametroId)
    {
        _svc = svc;
        _db = db;
    }

    [AllowAnonymous]
    [HttpGet("centro/{centroAcopioId:int}")]
    public async Task<ActionResult<IReadOnlyList<ParametroCalidadDto>>> GetByCentro(int centroAcopioId, CancellationToken cancellationToken)
    {
        var list = await _db.ParametrosCalidad
            .Where(p => p.CentroAcopioId == centroAcopioId)
            .OrderBy(p => p.Orden)
            .Select(p => new ParametroCalidadDto(p.ParametroId, p.CentroAcopioId, p.Nombre, p.Unidad, p.ValorMinimo, p.ValorMaximo, p.Descripcion, p.Orden))
            .ToListAsync(cancellationToken);

        return Ok(list);
    }
}
