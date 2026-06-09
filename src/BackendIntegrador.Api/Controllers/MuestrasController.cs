using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using BackendIntegrador.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendIntegrador.Api.Controllers;

[ApiController]
[Route("api/muestras")]
public sealed class MuestrasController : IntKeyCrudControllerBase<MuestraDto, CreateMuestraDto, UpdateMuestraDto>
{
    private readonly AppDbContext _db;

    public MuestrasController(
        ICrudService<MuestraDto, CreateMuestraDto, UpdateMuestraDto> svc,
        AppDbContext db)
        : base(svc, m => m.MuestraId)
    {
        _db = db;
    }

    [HttpGet("por-lote/{loteId:int}")]
    public async Task<ActionResult<IReadOnlyList<MuestraConEstadoDto>>> GetByLote(int loteId, CancellationToken cancellationToken)
    {
        var muestras = await _db.Muestras
            .AsNoTracking()
            .Include(m => m.Analisis)
            .Where(m => m.LoteId == loteId)
            .OrderByDescending(m => m.MuestraId)
            .ToListAsync(cancellationToken);

        var dtos = muestras.Select(m => new MuestraConEstadoDto(
            m.MuestraId, m.LoteId, m.TecnicoPorUsuarioId,
            m.FechaHoraToma, m.Analisis.Any()))
            .ToList() as IReadOnlyList<MuestraConEstadoDto>;

        return Ok(dtos);
    }
}
