using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using BackendIntegrador.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BackendIntegrador.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    private readonly IRepository<Usuario> _usuarioRepo;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthenticationService authService,
        IRepository<Usuario> usuarioRepo,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _usuarioRepo = usuarioRepo;
        _logger = logger;
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
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("POST /api/auth/forgot-password recibido.");

        try
        {
            await _authService.ForgotPasswordAsync(dto, cancellationToken);

            _logger.LogInformation(
                "POST /api/auth/forgot-password respondió 200 en {ElapsedMs}ms.",
                stopwatch.ElapsedMilliseconds);

            return Ok(new
            {
                message = "Si el correo existe, se enviaron instrucciones."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "POST /api/auth/forgot-password falló en {ElapsedMs}ms. ExceptionType={ExceptionType}",
                stopwatch.ElapsedMilliseconds,
                ex.GetType().Name);
            throw;
        }
    }

    [AllowAnonymous]
    [HttpPost("verify-reset-code")]
    public async Task<IActionResult> VerifyResetCode(
        VerifyResetCodeDto dto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("POST /api/auth/verify-reset-code recibido.");

        return Ok(new { token = dto.Token });
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordDto dto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("POST /api/auth/reset-password recibido.");

        await _authService.ResetPasswordAsync(dto, cancellationToken);

        return Ok(new { message = "Contraseña actualizada correctamente." });
    }
}
