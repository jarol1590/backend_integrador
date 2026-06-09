using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackendIntegrador.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/centros-acopio/{centroAcopioId:int}/gemelo")]
public sealed class CentroAcopioGemeloController : ControllerBase
{
    private readonly ICentroAcopioGemeloService _gemelo;
    private readonly IFincaGemeloAuthorizationService _auth;

    public CentroAcopioGemeloController(ICentroAcopioGemeloService gemelo, IFincaGemeloAuthorizationService auth)
    {
        _gemelo = gemelo;
        _auth = auth;
    }

    [HttpGet("riesgo-regional")]
    public async Task<ActionResult<CentroAcopioRiesgoRegionalDto>> GetRiesgoRegional(
        int centroAcopioId,
        CancellationToken cancellationToken)
    {
        await _auth.EnsureCanAccessCentroAsync(GetCurrentUsuarioId(), centroAcopioId, cancellationToken);
        return Ok(await _gemelo.GetRiesgoRegionalAsync(centroAcopioId, cancellationToken));
    }

    private int GetCurrentUsuarioId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim is null || !int.TryParse(claim.Value, out var usuarioId))
            throw new UnauthorizedAccessException("No se pudo obtener el ID del usuario.");
        return usuarioId;
    }
}
