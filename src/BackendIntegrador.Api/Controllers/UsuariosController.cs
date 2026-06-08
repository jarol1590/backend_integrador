using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackendIntegrador.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize]
public sealed class UsuariosController : ControllerBase
{
    private readonly IUsuarioFacadeService _facade;
    private readonly IUserManagementService _userManagement;

    public UsuariosController(IUsuarioFacadeService facade, IUserManagementService userManagement)
    {
        _facade = facade;
        _userManagement = userManagement;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UsuarioListadoDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _facade.GetListadoAsync(cancellationToken));

    [HttpGet("me")]
    public async Task<ActionResult<UsuarioPerfilBaseDto>> GetMe(CancellationToken cancellationToken)
    {
        var usuarioId = GetCurrentUsuarioId();
        var perfil = await _facade.GetPerfilAsync(usuarioId, cancellationToken);
        return perfil is null ? NotFound() : Ok(perfil);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UsuarioPerfilBaseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var perfil = await _facade.GetPerfilAsync(id, cancellationToken);
        return perfil is null ? NotFound() : Ok(perfil);
    }

    [AllowAnonymous]
    [HttpGet("public/{id:int}")]
    public async Task<ActionResult<ProvisionarUsuarioDto>> GetByIdPublic(int id, CancellationToken cancellationToken)
    {
        var input = await _facade.GetInputAsync(id, cancellationToken);
        return input is null ? NotFound() : Ok(input);
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<UsuarioPerfilBaseDto>> Create(
        [FromBody] ProvisionarUsuarioDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await _facade.ProvisionarAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.UsuarioId }, new
            {
                message = "Usuario creado correctamente",
                data = created,
                status = 201
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message, status = 400 });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] ActualizarUsuarioDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _facade.ActualizarAsync(id, dto, cancellationToken);
            return Ok(new
            {
                message = "Usuario actualizado correctamente",
                data = updated,
                status = 200
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Registro no encontrado", status = 404 });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message, status = 400 });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _facade.DesactivarAsync(id, cancellationToken);
            return Ok(new { message = "Usuario desactivado correctamente", status = 200 });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Registro no encontrado", status = 404 });
        }
    }

    [HttpPost("{id:int}/reset-password")]
    public async Task<IActionResult> ResetPassword(
        int id,
        [FromBody] ResetPasswordDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            await _userManagement.ResetUserPasswordAsync(id, dto.NewPassword, cancellationToken);
            return Ok(new { message = "Contraseña restablecida correctamente", status = 200 });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Registro no encontrado", status = 404 });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message, status = 400 });
        }
    }

    private int GetCurrentUsuarioId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim is null || !int.TryParse(claim.Value, out var usuarioId))
            throw new UnauthorizedAccessException("No se pudo obtener el ID del usuario.");
        return usuarioId;
    }
}
