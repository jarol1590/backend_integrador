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
        var usuarios = await _usuarioRepo.GetAllAsync(cancellationToken);
        var usuario = usuarios.FirstOrDefault(u => u.Email == dto.Email);

        if (usuario is null || !BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash))
            throw new UnauthorizedAccessException("Credenciales inválidas.");

        if (usuario.Estado != "activo")
            throw new UnauthorizedAccessException("El usuario no está activo.");

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
