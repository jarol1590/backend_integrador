using BackendIntegrador.Application.Dtos;

namespace BackendIntegrador.Application.Abstractions;

public interface IUserManagementService
{
    Task<UsuarioDto> ResetUserPasswordAsync(int usuarioId, string newPassword, CancellationToken cancellationToken = default);
}
