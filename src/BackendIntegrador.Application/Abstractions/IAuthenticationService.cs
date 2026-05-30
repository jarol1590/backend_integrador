using BackendIntegrador.Application.Dtos;

namespace BackendIntegrador.Application.Abstractions;

public interface IAuthenticationService
{
    Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);

    Task ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken cancellationToken = default);

    Task ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken = default);
}
