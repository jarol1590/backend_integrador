using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Common;
using BackendIntegrador.Application.Dtos;
using BackendIntegrador.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace BackendIntegrador.Infrastructure.Services;

internal sealed class AuthenticationService : IAuthenticationService
{
    private readonly IRepository<Usuario> _usuarioRepo;
    private readonly JwtSettings _jwtSettings;
    private readonly IEmailService _emailService;

    public AuthenticationService(
        IRepository<Usuario> usuarioRepo,
        JwtSettings jwtSettings,
        IEmailService emailService)
    {
        _usuarioRepo = usuarioRepo;
        _jwtSettings = jwtSettings;
        _emailService = emailService;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            throw new InvalidOperationException("Email y contraseña son requeridos.");

        var usuario = await _usuarioRepo.FirstOrDefaultAsync(u => u.Email == dto.Email, cancellationToken);

        if (usuario is null)
            throw new InvalidOperationException("Credenciales inválidas.");

        // Validar que el PasswordHash no esté vacío
        if (string.IsNullOrWhiteSpace(usuario.PasswordHash))
            throw new InvalidOperationException("El usuario no tiene una contraseña válida configurada.");

        // Intentar verificar la contraseña de forma segura
        bool passwordValid = false;
        try
        {
            passwordValid = BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash);
        }
        catch (Exception ex)
        {
            // Si hay error en BCrypt (ej: hash inválido), lanzar error
            throw new InvalidOperationException("Error al validar la contraseña. Por favor, intente de nuevo.", ex);
        }

        if (!passwordValid)
            throw new InvalidOperationException("Credenciales inválidas.");

        if (usuario.Estado != "activo")
            throw new InvalidOperationException("El usuario no está activo.");

        var token = GenerateJwtToken(usuario);

        return new AuthResponseDto(
            token,
            new UsuarioDto(usuario.UsuarioId, usuario.Email, usuario.Estado, usuario.FechaCreacion, usuario.CentroAcopioId)
        );
    }

    private string GenerateJwtToken(Usuario usuario)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
            new Claim(ClaimTypes.Email, usuario.Email)
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

    private string GeneratePasswordResetToken(Usuario usuario)
    {
        // Aquí podrías implementar un token de un solo uso para restablecer la contraseña
        // Por simplicidad, usaremos un JWT con una expiración corta
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim("purpose", "password_reset")
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1), // Token válido por 1 hora
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

        var resetToken = GeneratePasswordResetToken(usuario);

        //var link = $"http://localhost:5111/reset-password?token={resetToken}"; //Está quemado hay que corregirlo

        /*await _emailService.SendAsync(
            usuario.Email,
            "Recuperación de contraseña",
            $"""
                <h2>Recuperación de contraseña</h2>

                <p>Haz clic en el siguiente enlace:</p>

                <a href="{link}">
                    Restablecer contraseña
                </a>
                """);
        */

        Console.WriteLine($"[Simulación de envío de email] Enviar a: {usuario.Email}");
        Console.WriteLine($"Token: {resetToken}");
    }

    private ClaimsPrincipal ValidatePasswordResetToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

        var principal = tokenHandler.ValidateToken(
            token,
            new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),

                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,

                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,

                ValidateLifetime = true,

                ClockSkew = TimeSpan.Zero
            },
            out _);

        var purpose = principal.FindFirst("purpose")?.Value;

        if (purpose != "password_reset")
            throw new InvalidOperationException(
                "Token inválido para recuperación.");

        return principal;
    }

    public async Task ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Token) || string.IsNullOrWhiteSpace(dto.NewPassword))
            throw new InvalidOperationException("Token y nueva contraseña son requeridos.");

        var principal = ValidatePasswordResetToken(dto.Token);
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //var tokenHandler = new JwtSecurityTokenHandler();
        //var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

        if (!int.TryParse(userId, out var usuarioId))
            throw new InvalidOperationException(
                "Token inválido.");

        var usuario =
            await _usuarioRepo.FindAsync(
                new object[] { usuarioId },
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