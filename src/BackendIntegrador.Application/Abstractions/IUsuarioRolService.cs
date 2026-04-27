using BackendIntegrador.Application.Dtos;

namespace BackendIntegrador.Application.Abstractions;

public interface IUsuarioRolService
{
    Task<IReadOnlyList<UsuarioRolDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UsuarioRolDto?> GetAsync(int usuarioId, int rolId, CancellationToken cancellationToken = default);
    Task<UsuarioRolDto> CreateAsync(CreateUsuarioRolDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int usuarioId, int rolId, CancellationToken cancellationToken = default);
}
