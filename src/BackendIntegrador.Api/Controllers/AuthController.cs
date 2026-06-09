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

    public AuthController(IAuthenticationService authService, IRepository<Usuario> usuarioRepo)
    {
        _authService = authService;
        _usuarioRepo = usuarioRepo;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(dto, cancellationToken);
        return Ok(response);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto, CancellationToken cancellationToken)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (usuarioIdClaim is null || !int.TryParse(usuarioIdClaim.Value, out var usuarioId))
            throw new UnauthorizedAccessException("No se pudo obtener el ID del usuario.");

        var usuario = await _usuarioRepo.FindAsync(new object[] { usuarioId }, cancellationToken);
        if (usuario is null)
            throw new KeyNotFoundException("Usuario no encontrado.");

        if (string.IsNullOrWhiteSpace(usuario.PasswordHash) || !BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, usuario.PasswordHash))
            throw new InvalidOperationException("La contraseña actual es incorrecta.");

        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _usuarioRepo.UpdateAsync(usuario, cancellationToken);

        return Ok(new { message = "Contraseña actualizada exitosamente." });
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
    [HttpPost("verify-reset-code")]
    public async Task<IActionResult> VerifyResetCode(
        VerifyResetCodeDto dto,
        CancellationToken cancellationToken)
    {
        await _authService.VerifyResetCodeAsync(
            dto,
            cancellationToken);

        return Ok(new { token = dto.Token });
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
