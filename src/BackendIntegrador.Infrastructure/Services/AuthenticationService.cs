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

    public AuthenticationService(
        IRepository<Usuario> usuarioRepo,
        JwtSettings jwtSettings)
    {
        _usuarioRepo = usuarioRepo;
        _jwtSettings = jwtSettings;
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
}
