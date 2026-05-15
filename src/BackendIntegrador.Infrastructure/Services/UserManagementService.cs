using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using BackendIntegrador.Domain.Entities;

namespace BackendIntegrador.Infrastructure.Services;

internal sealed class UserManagementService : IUserManagementService
{
    private readonly IRepository<Usuario> _usuarioRepo;

    public UserManagementService(IRepository<Usuario> usuarioRepo)
    {
        _usuarioRepo = usuarioRepo;
    }

    public async Task<UsuarioDto> ResetUserPasswordAsync(int usuarioId, string newPassword, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(newPassword))
            throw new InvalidOperationException("La nueva contraseña no puede estar vacía.");

        var usuario = await _usuarioRepo.FindAsync(new object[] { usuarioId }, cancellationToken);
        if (usuario is null)
            throw new KeyNotFoundException($"Usuario con id {usuarioId} no existe.");

        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _usuarioRepo.UpdateAsync(usuario, cancellationToken);

        return new UsuarioDto(usuario.UsuarioId, usuario.Email, usuario.Estado, usuario.FechaCreacion, usuario.CentroAcopioId);
    }
}
