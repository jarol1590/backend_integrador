using System.Collections.Concurrent;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Common;
using BackendIntegrador.Application.Dtos;
using BackendIntegrador.Domain.Entities;
using BackendIntegrador.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace BackendIntegrador.Infrastructure.Services;

internal sealed class AuthenticationService : IAuthenticationService
{
    private static readonly ConcurrentDictionary<string, PasswordResetCode> _resetCodes = new();

    private readonly AppDbContext _db;
    private readonly IRepository<Usuario> _usuarioRepo;
    private readonly JwtSettings _jwtSettings;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        IRepository<Usuario> usuarioRepo,
        AppDbContext db,
        JwtSettings jwtSettings,
        IEmailService emailService,
        ILogger<AuthenticationService> logger)
    {
        _db = db;
        _usuarioRepo = usuarioRepo;
        _jwtSettings = jwtSettings;
        _emailService = emailService;
        _logger = logger;
    }

    private sealed record PasswordResetCode(int UsuarioId, DateTime ExpiresAt);

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            throw new InvalidOperationException("Email y contraseña son requeridos.");

        var usuario = await _db.Usuarios
            .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
            .FirstOrDefaultAsync(u => u.Email == dto.Email, cancellationToken);

        if (usuario is null)
            throw new InvalidOperationException("Usuario no registrado, no puedes iniciar sesión.");

        if (string.IsNullOrWhiteSpace(usuario.PasswordHash))
            throw new InvalidOperationException("El usuario no tiene una contraseña válida configurada.");

        bool passwordValid;
        try
        {
            passwordValid = BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error al validar la contraseña. Por favor, intente de nuevo.", ex);
        }

        if (!passwordValid)
            throw new InvalidOperationException("Contraseña incorrecta.");

        if (usuario.Estado != "activo")
            throw new InvalidOperationException("Usuario inactivo, no puedes iniciar sesión.");

        var rol = usuario.UsuarioRoles.Select(ur => ur.Rol).FirstOrDefault();
        var rolNombre = rol?.Nombre ?? "Sin rol";
        var tipoUsuario = UsuarioRoleTypes.ResolveTipoFromRolNombre(rolNombre) ?? "sin_asignar";

        var token = GenerateJwtToken(usuario, rolNombre);

        return new AuthResponseDto(
            token,
            new AuthUsuarioDto(
                usuario.UsuarioId,
                usuario.Email,
                usuario.Estado,
                usuario.FechaCreacion,
                tipoUsuario,
                rolNombre,
                usuario.CentroAcopioId));
    }

    private string GenerateJwtToken(Domain.Entities.Usuario usuario, string rolNombre)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, rolNombre)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var maskedEmail = MaskEmail(dto.Email);

        _logger.LogInformation("ForgotPassword iniciado. Email={MaskedEmail}", maskedEmail);

        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new InvalidOperationException("Email es requerido.");

        var usuario = await _usuarioRepo.FirstOrDefaultAsync(u => u.Email == dto.Email, cancellationToken);
        if (usuario is null)
        {
            _logger.LogInformation(
                "ForgotPassword finalizado en {ElapsedMs}ms. Usuario no encontrado para Email={MaskedEmail}",
                stopwatch.ElapsedMilliseconds,
                maskedEmail);
            return; // No revelar que el email no existe
        }

        var code = Random.Shared.Next(10000, 99999).ToString();
        _resetCodes[code] = new PasswordResetCode(usuario.UsuarioId, DateTime.UtcNow.AddHours(1));

        _logger.LogInformation(
            "ForgotPassword: código generado para UsuarioId={UsuarioId}, Email={MaskedEmail}. Iniciando envío de correo.",
            usuario.UsuarioId,
            maskedEmail);

        try
        {
            await _emailService.SendAsync(
                usuario.Email,
                "Recuperación de contraseña",
                $"""
                <h2>Recuperación de contraseña</h2>

                <p>Utiliza el siguiente código para restablecer tu contraseña:</p>

                <h1 style="letter-spacing: 8px; text-align: center;">{code}</h1>

                <p>Este código expirará en 1 hora.</p>
                """);

            _logger.LogInformation(
                "ForgotPassword completado en {ElapsedMs}ms. Correo enviado a UsuarioId={UsuarioId}, Email={MaskedEmail}",
                stopwatch.ElapsedMilliseconds,
                usuario.UsuarioId,
                maskedEmail);
        }
        catch (Exception ex)
        {
            _resetCodes.TryRemove(code, out _);

            _logger.LogError(
                ex,
                "ForgotPassword falló en {ElapsedMs}ms al enviar correo. UsuarioId={UsuarioId}, Email={MaskedEmail}, ExceptionType={ExceptionType}",
                stopwatch.ElapsedMilliseconds,
                usuario.UsuarioId,
                maskedEmail,
                ex.GetType().Name);

            throw;
        }
    }

    public Task VerifyResetCodeAsync(VerifyResetCodeDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("VerifyResetCode iniciado. TokenLength={TokenLength}", dto.Token?.Length ?? 0);

        if (string.IsNullOrWhiteSpace(dto.Token))
            throw new InvalidOperationException("Código requerido.");

        if (!_resetCodes.TryGetValue(dto.Token, out var resetCode))
        {
            _logger.LogWarning("VerifyResetCode: código inválido.");
            throw new InvalidOperationException("Código inválido.");
        }

        if (resetCode.ExpiresAt < DateTime.UtcNow)
        {
            _resetCodes.TryRemove(dto.Token, out _);
            _logger.LogWarning(
                "VerifyResetCode: código expirado para UsuarioId={UsuarioId}",
                resetCode.UsuarioId);
            throw new InvalidOperationException("El código ha expirado.");
        }

        _logger.LogInformation(
            "VerifyResetCode exitoso para UsuarioId={UsuarioId}",
            resetCode.UsuarioId);

        return Task.CompletedTask;
    }

    public async Task ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("ResetPassword iniciado. TokenLength={TokenLength}", dto.Token?.Length ?? 0);

        if (string.IsNullOrWhiteSpace(dto.Token) || string.IsNullOrWhiteSpace(dto.NewPassword))
            throw new InvalidOperationException("Código y nueva contraseña son requeridos.");

        if (!_resetCodes.TryGetValue(dto.Token, out var resetCode))
        {
            _logger.LogWarning("ResetPassword: código inválido.");
            throw new InvalidOperationException("Código inválido.");
        }

        if (resetCode.ExpiresAt < DateTime.UtcNow)
        {
            _resetCodes.TryRemove(dto.Token, out _);
            _logger.LogWarning(
                "ResetPassword: código expirado para UsuarioId={UsuarioId}",
                resetCode.UsuarioId);
            throw new InvalidOperationException("El código ha expirado.");
        }

        _resetCodes.TryRemove(dto.Token, out _);

        var usuario =
            await _usuarioRepo.FindAsync(
                new object[] { resetCode.UsuarioId },
                cancellationToken);

        if (usuario is null)
        {
            _logger.LogError(
                "ResetPassword: usuario no encontrado. UsuarioId={UsuarioId}",
                resetCode.UsuarioId);
            throw new InvalidOperationException("Usuario no encontrado.");
        }

        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

        await _usuarioRepo.UpdateAsync(usuario, cancellationToken);

        _logger.LogInformation(
            "ResetPassword completado en {ElapsedMs}ms para UsuarioId={UsuarioId}, Email={MaskedEmail}",
            stopwatch.ElapsedMilliseconds,
            usuario.UsuarioId,
            MaskEmail(usuario.Email));
    }

    private static string MaskEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return "(vacío)";

        var at = email.IndexOf('@');
        if (at <= 0)
            return "***";

        return email[0] + "***" + email[at..];
    }
}