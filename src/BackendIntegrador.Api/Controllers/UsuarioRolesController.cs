using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace BackendIntegrador.Api.Controllers;

[ApiController]
[Route("api/usuario-roles")]
public sealed class UsuarioRolesController : ControllerBase
{
    private readonly IUsuarioRolService _svc;

    public UsuarioRolesController(IUsuarioRolService svc) => _svc = svc;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UsuarioRolDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _svc.GetAllAsync(cancellationToken));

    [HttpGet("{usuarioId:int}/{rolId:int}")]
    public async Task<ActionResult<UsuarioRolDto>> Get(int usuarioId, int rolId, CancellationToken cancellationToken)
    {
        var item = await _svc.GetAsync(usuarioId, rolId, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<UsuarioRolDto>> Create([FromBody] CreateUsuarioRolDto dto, CancellationToken cancellationToken)
    {
        var created = await _svc.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(Get), new { usuarioId = created.UsuarioId, rolId = created.RolId }, created);
    }

    [HttpDelete("{usuarioId:int}/{rolId:int}")]
    public async Task<IActionResult> Delete(int usuarioId, int rolId, CancellationToken cancellationToken)
    {
        try
        {
            await _svc.DeleteAsync(usuarioId, rolId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
