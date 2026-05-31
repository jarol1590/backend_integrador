using BackendIntegrador.Application.Dtos;

namespace BackendIntegrador.Application.Abstractions;

public interface IUsuarioFacadeService
{
    Task<IReadOnlyList<UsuarioListadoDto>> GetListadoAsync(CancellationToken cancellationToken = default);
    Task<UsuarioPerfilDto?> GetPerfilAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<UsuarioPerfilDto> ProvisionarAsync(ProvisionarUsuarioDto dto, CancellationToken cancellationToken = default);
    Task<UsuarioPerfilDto> ActualizarAsync(int usuarioId, ActualizarUsuarioDto dto, CancellationToken cancellationToken = default);
    Task DesactivarAsync(int usuarioId, CancellationToken cancellationToken = default);
}
