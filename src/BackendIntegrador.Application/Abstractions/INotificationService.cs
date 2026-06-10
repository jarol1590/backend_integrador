namespace BackendIntegrador.Application.Abstractions;

public interface INotificationService
{
    Task SendToUserAsync(int usuarioId, string title, string body, object? data = null, CancellationToken cancellationToken = default);
}
