using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackendIntegrador.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/fincas/{fincaId:int}/gemelo")]
public sealed class FincaGemeloController : ControllerBase
{
    private readonly IFincaGemeloService _gemelo;
    private readonly IFincaGemeloAuthorizationService _auth;

    public FincaGemeloController(IFincaGemeloService gemelo, IFincaGemeloAuthorizationService auth)
    {
        _gemelo = gemelo;
        _auth = auth;
    }

    [HttpGet("estado")]
    public async Task<ActionResult<FincaGemeloEstadoDto>> GetEstado(int fincaId, CancellationToken cancellationToken)
    {
        await AuthorizeFincaAsync(fincaId, cancellationToken);
        return Ok(await _gemelo.GetEstadoAsync(fincaId, cancellationToken));
    }

    [HttpGet("clima")]
    public async Task<ActionResult<IReadOnlyList<LecturaClimaticaDto>>> GetClima(
        int fincaId,
        [FromQuery] DateOnly? desde,
        [FromQuery] DateOnly? hasta,
        CancellationToken cancellationToken)
    {
        await AuthorizeFincaAsync(fincaId, cancellationToken);
        return Ok(await _gemelo.GetClimaAsync(fincaId, desde, hasta, cancellationToken));
    }

    [HttpGet("predicciones")]
    public async Task<ActionResult<IReadOnlyList<PrediccionGemeloDto>>> GetPredicciones(
        int fincaId,
        [FromQuery] int? horizonteDias,
        CancellationToken cancellationToken)
    {
        await AuthorizeFincaAsync(fincaId, cancellationToken);
        return Ok(await _gemelo.GetPrediccionesAsync(fincaId, horizonteDias, cancellationToken));
    }

    [HttpGet("alertas")]
    public async Task<ActionResult<IReadOnlyList<AlertaGemeloDto>>> GetAlertas(
        int fincaId,
        [FromQuery] bool? activas,
        CancellationToken cancellationToken)
    {
        await AuthorizeFincaAsync(fincaId, cancellationToken);
        return Ok(await _gemelo.GetAlertasAsync(fincaId, activas, cancellationToken));
    }

    [HttpPost("sincronizar")]
    public async Task<ActionResult<SincronizarGemeloResultDto>> Sincronizar(int fincaId, CancellationToken cancellationToken)
    {
        await AuthorizeFincaAsync(fincaId, cancellationToken);
        try
        {
            var result = await _gemelo.SincronizarAsync(fincaId, cancellationToken);
            return Ok(new { message = "Gemelo digital sincronizado correctamente", data = result, status = 200 });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message, status = 400 });
        }
    }

    [HttpPatch("alertas/{alertaId:int}/leida")]
    public async Task<IActionResult> MarcarAlertaLeida(int fincaId, int alertaId, CancellationToken cancellationToken)
    {
        await AuthorizeFincaAsync(fincaId, cancellationToken);
        try
        {
            await _gemelo.MarcarAlertaLeidaAsync(fincaId, alertaId, cancellationToken);
            return Ok(new { message = "Alerta marcada como leída", status = 200 });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Alerta no encontrada", status = 404 });
        }
    }

    private async Task AuthorizeFincaAsync(int fincaId, CancellationToken cancellationToken)
    {
        await _auth.EnsureCanAccessFincaAsync(GetCurrentUsuarioId(), fincaId, cancellationToken);
    }

    private int GetCurrentUsuarioId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim is null || !int.TryParse(claim.Value, out var usuarioId))
            throw new UnauthorizedAccessException("No se pudo obtener el ID del usuario.");
        return usuarioId;
    }
}
