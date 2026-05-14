using BackendIntegrador.Application.Dtos;

namespace BackendIntegrador.Application.Abstractions;

public interface IAuthenticationService
{
    Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
}
