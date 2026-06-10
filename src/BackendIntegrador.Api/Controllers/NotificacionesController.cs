using BackendIntegrador.Application.Dtos;
using BackendIntegrador.Domain.Entities;
using BackendIntegrador.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendIntegrador.Api.Controllers;

[ApiController]
[Route("api/notificaciones")]
public sealed class NotificacionesController : ControllerBase
{
    private readonly AppDbContext _db;

    public NotificacionesController(AppDbContext db)
    {
        _db = db;
    }

    [AllowAnonymous]
    [HttpPost("registrar-token")]
    public async Task<IActionResult> RegistrarToken([FromBody] RegisterDeviceTokenDto dto, CancellationToken cancellationToken)
    {
        var existing = await _db.DeviceTokens
            .FirstOrDefaultAsync(dt => dt.UsuarioId == dto.UsuarioId && dt.Token == dto.Token, cancellationToken);

        if (existing is not null)
        {
            existing.Platform = dto.Platform;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            _db.DeviceTokens.Add(new DeviceToken
            {
                UsuarioId = dto.UsuarioId,
                Token = dto.Token,
                Platform = dto.Platform,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Token registrado correctamente" });
    }

    [AllowAnonymous]
    [HttpDelete("token")]
    public async Task<IActionResult> EliminarToken([FromQuery] string token, CancellationToken cancellationToken)
    {
        var entity = await _db.DeviceTokens
            .FirstOrDefaultAsync(dt => dt.Token == token, cancellationToken);

        if (entity is not null)
        {
            _db.DeviceTokens.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
        }

        return NoContent();
    }
}
