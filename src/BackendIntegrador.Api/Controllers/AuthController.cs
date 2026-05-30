using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using BackendIntegrador.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackendIntegrador.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    private readonly IRepository<Usuario> _usuarioRepo;
    private readonly IUserManagementService _userManagementService;

    public AuthController(
        IAuthenticationService authService,
        IRepository<Usuario> usuarioRepo,
        IUserManagementService userManagementService)
    {
        _authService = authService;
        _usuarioRepo = usuarioRepo;
        _userManagementService = userManagementService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authService.LoginAsync(dto, cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            throw ex;
        }
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (usuarioIdClaim is null || !int.TryParse(usuarioIdClaim.Value, out var usuarioId))
                throw new UnauthorizedAccessException("No se pudo obtener el ID del usuario.");

            var usuario = await _usuarioRepo.FindAsync(new object[] { usuarioId }, cancellationToken);
            if (usuario is null)
                throw new KeyNotFoundException("Usuario no encontrado.");

            // Validar contraseña actual
            if (string.IsNullOrWhiteSpace(usuario.PasswordHash) || !BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, usuario.PasswordHash))
                throw new InvalidOperationException("La contraseña actual es incorrecta.");

            // Actualizar contraseña
            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _usuarioRepo.UpdateAsync(usuario, cancellationToken);

            return Ok(new { message = "Contraseña actualizada exitosamente." });
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    [Authorize]
    [HttpPost("reset-password/{usuarioId:int}")]
    public async Task<ActionResult<UsuarioDto>> ResetPassword(int usuarioId, [FromBody] ResetPasswordDto dto, CancellationToken cancellationToken)
    {
        try
        {
            // TODO: Aquí puedes agregar validación de rol de administrador
            // var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            // if (userRole != "Admin") throw new UnauthorizedAccessException("Solo administradores pueden resetear contraseñas.");

            var usuario = await _userManagementService.ResetUserPasswordAsync(usuarioId, dto.NewPassword, cancellationToken);
            return Ok(usuario);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordDto dto,
        CancellationToken cancellationToken)
    {
        await _authService.ForgotPasswordAsync(
            dto,
            cancellationToken);

        return Ok(new
        {
            message =
                "Si el correo existe, se enviaron instrucciones."
        });
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordDto dto,
        CancellationToken cancellationToken)
    {
        await _authService.ResetPasswordAsync(
            dto,
            cancellationToken);

        return Ok(new
        {
            message = "Contraseña actualizada correctamente."
        });
    }
}
