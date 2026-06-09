using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using BackendIntegrador.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendIntegrador.Api.Controllers;

[ApiController]
[Route("api/analisis-calidad")]
public sealed class AnalisisCalidadController : IntKeyCrudControllerBase<AnalisisCalidadDto, CreateAnalisisCalidadDto, UpdateAnalisisCalidadDto>
{
    private readonly AppDbContext _db;

    public AnalisisCalidadController(
        ICrudService<AnalisisCalidadDto, CreateAnalisisCalidadDto, UpdateAnalisisCalidadDto> svc,
        AppDbContext db)
        : base(svc, a => a.AnalisisId)
    {
        _db = db;
    }

    [AllowAnonymous]
    [HttpGet("por-finca/{fincaId:int}")]
    public async Task<ActionResult<IReadOnlyList<AnalisisPorFincaDto>>> GetByFinca(int fincaId, CancellationToken cancellationToken)
    {
        var finca = await _db.Fincas
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.FincaId == fincaId, cancellationToken);

        if (finca is null)
            return NotFound(new { message = "Finca no encontrada" });

        var analisis = await _db.AnalisisCalidad
            .AsNoTracking()
            .Include(a => a.Muestra)
                .ThenInclude(m => m.Lote)
                    .ThenInclude(l => l.Ordeno)
            .Include(a => a.Resultados)
                .ThenInclude(r => r.Parametro)
            .Where(a => a.Muestra.Lote.Ordeno.FincaId == fincaId)
            .OrderByDescending(a => a.FechaHoraAnalisis)
            .ToListAsync(cancellationToken);

        var result = analisis.Select(a => new AnalisisPorFincaDto(
            a.AnalisisId,
            a.Muestra.LoteId,
            finca.Nombre,
            a.FechaHoraAnalisis,
            a.Resultados.Select(r =>
            {
                var val = (double)r.ValorResultado;
                double? min = r.Parametro.ValorMinimo is not null ? (double)r.Parametro.ValorMinimo : null;
                double? max = r.Parametro.ValorMaximo is not null ? (double)r.Parametro.ValorMaximo : null;
                var enRango = min is null || max is null || (val >= min && val <= max);
                return new ResultadoVisualDto(
                    r.Parametro.Nombre,
                    r.Parametro.Unidad,
                    val,
                    min,
                    max,
                    enRango);
            }).ToList() as IReadOnlyList<ResultadoVisualDto>
        )).ToList() as IReadOnlyList<AnalisisPorFincaDto>;

        return Ok(result);
    }
}
