using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Common;
using BackendIntegrador.Application.Dtos;
using BackendIntegrador.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BackendIntegrador.Infrastructure.Services;

internal sealed class AuthenticationService : IAuthenticationService
{
    private readonly AppDbContext _db;
    private readonly JwtSettings _jwtSettings;

    public AuthenticationService(AppDbContext db, JwtSettings jwtSettings)
    {
        _db = db;
        _jwtSettings = jwtSettings;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            throw new InvalidOperationException("Email y contraseña son requeridos.");

        var usuario = await _db.Usuarios
            .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
            .FirstOrDefaultAsync(u => u.Email == dto.Email, cancellationToken);

        if (usuario is null)
            throw new InvalidOperationException("Credenciales inválidas.");

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
            throw new InvalidOperationException("Credenciales inválidas.");

        if (usuario.Estado != "activo")
            throw new InvalidOperationException("El usuario no está activo.");

        var roleNames = usuario.UsuarioRoles.Select(ur => ur.Rol.Nombre).ToList();
        var token = GenerateJwtToken(usuario, roleNames);
        var alcance = UsuarioAlcanceHelper.DerivarTipoUsuario(roleNames, usuario.CentroAcopioId);

        return new AuthResponseDto(
            token,
            new AuthUsuarioDto(
                usuario.UsuarioId,
                usuario.Email,
                usuario.Estado,
                usuario.FechaCreacion,
                usuario.CentroAcopioId,
                roleNames,
                alcance));
    }

    private string GenerateJwtToken(Domain.Entities.Usuario usuario, IReadOnlyList<string> roleNames)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
            new(ClaimTypes.Email, usuario.Email)
        };

        foreach (var role in roleNames)
            claims.Add(new Claim(ClaimTypes.Role, role));

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
