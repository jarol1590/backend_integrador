using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using BackendIntegrador.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BackendIntegrador.Api.Controllers;

[ApiController]
[Route("api/transportes")]
public sealed class TransportesController : IntKeyCrudControllerBase<TransporteDto, CreateTransporteDto, UpdateTransporteDto>
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notificationService;

    public TransportesController(
        ICrudService<TransporteDto, CreateTransporteDto, UpdateTransporteDto> svc,
        AppDbContext db,
        INotificationService notificationService)
        : base(svc, t => t.TransporteId)
    {
        _db = db;
        _notificationService = notificationService;
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

        try
        {
            var lotesInfo = await _db.Lotes
                .AsNoTracking()
                .Where(l => l.TransporteId == id)
                .Select(l => new
                {
                    l.Codigo,
                    l.Ordeno.Finca.Productor.UsuarioId
                })
                .Distinct()
                .ToListAsync(cancellationToken);

            foreach (var lote in lotesInfo)
            {
                await _notificationService.SendToUserAsync(
                    lote.UsuarioId,
                    "Lote entregado",
                    $"Tu lote {lote.Codigo} ha sido entregado en el centro de acopio",
                    new { screen = "lotes", loteId = id },
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<TransportesController>>();
            logger.LogWarning(ex, "Failed to send push notification for completed transport {TransporteId}", id);
        }

        return Ok(new TransporteDto(
            entity.TransporteId, entity.PlacaVehiculo, entity.FechaHoraSalida,
            entity.FechaHoraEntrada, entity.TemperaturaInicio));
    }
}
