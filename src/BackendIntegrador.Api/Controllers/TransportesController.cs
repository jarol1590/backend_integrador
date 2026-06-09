using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using BackendIntegrador.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendIntegrador.Api.Controllers;

[ApiController]
[Route("api/transportes")]
public sealed class TransportesController : IntKeyCrudControllerBase<TransporteDto, CreateTransporteDto, UpdateTransporteDto>
{
    private readonly AppDbContext _db;

    public TransportesController(
        ICrudService<TransporteDto, CreateTransporteDto, UpdateTransporteDto> svc,
        AppDbContext db)
        : base(svc, t => t.TransporteId)
    {
        _db = db;
    }

    [AllowAnonymous]
    [HttpGet("por-centro/{centroAcopioId:int}")]
    public async Task<ActionResult<IReadOnlyList<TransporteDto>>> GetByCentro(int centroAcopioId, CancellationToken cancellationToken)
    {
        var transportes = await _db.Lotes
            .AsNoTracking()
            .Where(l => l.CentroAcopioId == centroAcopioId && l.TransporteId != null)
            .Select(l => l.Transporte!)
            .Distinct()
            .ToListAsync(cancellationToken);

        var dtos = transportes.Select(t => new TransporteDto(
            t.TransporteId, t.PlacaVehiculo, t.FechaHoraSalida,
            t.FechaHoraEntrada, t.TemperaturaInicio))
            .ToList() as IReadOnlyList<TransporteDto>;

        return Ok(dtos);
    }

    [HttpPost("{id}/completar")]
    public async Task<IActionResult> Completar(int id, CancellationToken cancellationToken)
    {
        var entity = await _db.Transportes.FindAsync(new object[] { id }, cancellationToken);
        if (entity is null)
            return NotFound(new { message = "Transporte no encontrado", status = 404 });

        entity.FechaHoraEntrada = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new TransporteDto(
            entity.TransporteId, entity.PlacaVehiculo, entity.FechaHoraSalida,
            entity.FechaHoraEntrada, entity.TemperaturaInicio));
    }
}
