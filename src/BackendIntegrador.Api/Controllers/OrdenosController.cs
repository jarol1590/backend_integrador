using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using BackendIntegrador.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendIntegrador.Api.Controllers;

[ApiController]
[Route("api/ordenos")]
public sealed class OrdenosController : IntKeyCrudControllerBase<OrdenoDto, CreateOrdenoDto, UpdateOrdenoDto>
{
    private readonly AppDbContext _db;

    public OrdenosController(
        ICrudService<OrdenoDto, CreateOrdenoDto, UpdateOrdenoDto> svc,
        AppDbContext db)
        : base(svc, o => o.OrdenoId)
    {
        _db = db;
    }

    [AllowAnonymous]
    [HttpGet("por-finca/{fincaId:int}")]
    public async Task<ActionResult<IReadOnlyList<OrdenoDto>>> GetByFinca(int fincaId, CancellationToken cancellationToken)
    {
        var ordenos = await _db.Ordenos
            .AsNoTracking()
            .Where(o => o.FincaId == fincaId)
            .OrderByDescending(o => o.FechaHoraInicio)
            .ToListAsync(cancellationToken);

        var dtos = ordenos.Select(o => new OrdenoDto(
            o.OrdenoId, o.Codigo, o.FechaHoraInicio, o.FechaHoraFin,
            o.VolumenLitros, o.FincaId))
            .ToList() as IReadOnlyList<OrdenoDto>;

        return Ok(dtos);
    }
}
