using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using BackendIntegrador.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendIntegrador.Api.Controllers;

[ApiController]
[Route("api/lotes")]
public sealed class LotesController : IntKeyCrudControllerBase<LoteDto, CreateLoteDto, UpdateLoteDto>
{
    private readonly AppDbContext _db;

    public LotesController(
        ICrudService<LoteDto, CreateLoteDto, UpdateLoteDto> svc,
        AppDbContext db)
        : base(svc, l => l.LoteId)
    {
        _db = db;
    }

    [AllowAnonymous]
    [HttpGet("por-centro/{centroAcopioId:int}")]
    public async Task<ActionResult<IReadOnlyList<LoteDto>>> GetByCentro(int centroAcopioId, CancellationToken cancellationToken)
    {
        var lotes = await _db.Lotes
            .AsNoTracking()
            .Include(l => l.Transporte)
            .Where(l => l.CentroAcopioId == centroAcopioId
                     && l.Transporte != null
                     && l.Transporte.FechaHoraEntrada != null)
            .OrderByDescending(l => l.LoteId)
            .ToListAsync(cancellationToken);

        var dtos = lotes.Select(l => new LoteDto(
            l.LoteId, l.OrdenoId, l.CentroAcopioId,
            l.VolumenCapturadoLitros, l.TransporteId,
            l.Transporte!.FechaHoraEntrada))
            .ToList() as IReadOnlyList<LoteDto>;

        return Ok(dtos);
    }

    [AllowAnonymous]
    [HttpGet("por-finca/{fincaId:int}")]
    public async Task<ActionResult<IReadOnlyList<LoteDto>>> GetByFinca(int fincaId, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[DEBUG] GET /api/lotes/por-finca/{fincaId}");

        Console.WriteLine("[DEBUG] Total lotes en DB:");
        foreach (var l in await _db.Lotes.AsNoTracking().ToListAsync(cancellationToken))
        {
            Console.WriteLine($"  Lote #{l.LoteId}: OrdenoId={l.OrdenoId}, Centro={l.CentroAcopioId}, Transporte={l.TransporteId}");
        }

        Console.WriteLine("[DEBUG] Total ordenos en DB:");
        foreach (var o in await _db.Ordenos.AsNoTracking().ToListAsync(cancellationToken))
        {
            Console.WriteLine($"  Ordeno #{o.OrdenoId}: FincaId={o.FincaId}");
        }

        var lotes = await _db.Lotes
            .AsNoTracking()
            .Include(l => l.Ordeno)
            .Include(l => l.Transporte)
            .Where(l => l.Ordeno.FincaId == fincaId)
            .OrderByDescending(l => l.LoteId)
            .ToListAsync(cancellationToken);

        Console.WriteLine($"[DEBUG] Found {lotes.Count} lotes for finca {fincaId}");

        var dtos = lotes.Select(l => new LoteDto(
            l.LoteId, l.OrdenoId, l.CentroAcopioId,
            l.VolumenCapturadoLitros, l.TransporteId,
            l.Transporte?.FechaHoraEntrada))
            .ToList() as IReadOnlyList<LoteDto>;

        return Ok(dtos);
    }
}
