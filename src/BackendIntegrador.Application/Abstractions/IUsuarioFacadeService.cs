using BackendIntegrador.Application.Dtos;

namespace BackendIntegrador.Application.Abstractions;

public interface IUsuarioFacadeService
{
    Task<IReadOnlyList<UsuarioListadoDto>> GetListadoAsync(CancellationToken cancellationToken = default);
    Task<UsuarioPerfilBaseDto?> GetPerfilAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<ProvisionarUsuarioDto?> GetInputAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<UsuarioPerfilBaseDto> ProvisionarAsync(ProvisionarUsuarioDto dto, CancellationToken cancellation = default);
    Task<UsuarioPerfilBaseDto> ActualizarAsync(int usuarioId, ActualizarUsuarioDto dto, CancellationToken cancellation = default);
    Task DesactivarAsync(int usuarioId, CancellationToken cancellation = default);
}
