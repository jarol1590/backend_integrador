using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Common;
using BackendIntegrador.Application.Dtos;
using BackendIntegrador.Domain.Entities;
using BackendIntegrador.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BackendIntegrador.Infrastructure.Services;

internal sealed class AuthenticationService : IAuthenticationService
{
    private static readonly ConcurrentDictionary<string, PasswordResetCode> _resetCodes = new();

    private readonly AppDbContext _db;
    private readonly IRepository<Usuario> _usuarioRepo;
    private readonly JwtSettings _jwtSettings;
    private readonly IEmailService _emailService;

    public AuthenticationService(
        IRepository<Usuario> usuarioRepo,
        AppDbContext db,
        JwtSettings jwtSettings,
        IEmailService emailService)
    {
        _db = db;
        _usuarioRepo = usuarioRepo;
        _jwtSettings = jwtSettings;
        _emailService = emailService;
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
        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new InvalidOperationException("Email es requerido.");

        var usuario = await _usuarioRepo.FirstOrDefaultAsync(u => u.Email == dto.Email, cancellationToken);
        if (usuario is null)
            return; // No revelar que el email no existe

        // Generar código de 5 dígitos
        var code = Random.Shared.Next(10000, 99999).ToString();
        _resetCodes[code] = new PasswordResetCode(usuario.UsuarioId, DateTime.UtcNow.AddHours(1));

        await _emailService.SendAsync(
        usuario.Email,
        "Recuperación de contraseña",
        $"""
        <h2>Recuperación de contraseña</h2>

        <p>Utiliza el siguiente código para restablecer tu contraseña:</p>

        <h1 style="letter-spacing: 8px; text-align: center;">{code}</h1>

        <p>Este código expirará en 1 hora.</p>
        """);
    }

    public Task VerifyResetCodeAsync(VerifyResetCodeDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Token))
            throw new InvalidOperationException("Código requerido.");

        if (!_resetCodes.TryGetValue(dto.Token, out var resetCode))
            throw new InvalidOperationException("Código inválido.");

        if (resetCode.ExpiresAt < DateTime.UtcNow)
        {
            _resetCodes.TryRemove(dto.Token, out _);
            throw new InvalidOperationException("El código ha expirado.");
        }

        return Task.CompletedTask;
    }

    public async Task ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Token) || string.IsNullOrWhiteSpace(dto.NewPassword))
            throw new InvalidOperationException("Código y nueva contraseña son requeridos.");

        if (!_resetCodes.TryGetValue(dto.Token, out var resetCode))
            throw new InvalidOperationException("Código inválido.");

        if (resetCode.ExpiresAt < DateTime.UtcNow)
        {
            _resetCodes.TryRemove(dto.Token, out _);
            throw new InvalidOperationException("El código ha expirado.");
        }

        _resetCodes.TryRemove(dto.Token, out _);

        var usuario =
            await _usuarioRepo.FindAsync(
                new object[] { resetCode.UsuarioId },
                cancellationToken);

        if (usuario is null)
            throw new InvalidOperationException(
                "Usuario no encontrado.");

        usuario.PasswordHash =
            BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

        await _usuarioRepo.UpdateAsync(
            usuario,
            cancellationToken);
    }
}