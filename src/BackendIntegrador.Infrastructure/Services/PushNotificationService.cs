using System.Net.Http.Json;
using System.Text.Json.Serialization;
using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BackendIntegrador.Infrastructure.Services;

internal sealed class PushNotificationService : INotificationService
{
    private const string ExpoPushUrl = "https://exp.host/--/api/v2/push/send";

    private readonly AppDbContext _db;
    private readonly HttpClient _httpClient;
    private readonly ILogger<PushNotificationService> _logger;

    public PushNotificationService(AppDbContext db, HttpClient httpClient, ILogger<PushNotificationService> logger)
    {
        _db = db;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task SendToUserAsync(int usuarioId, string title, string body, object? data = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var tokens = await _db.DeviceTokens
                .AsNoTracking()
                .Where(dt => dt.UsuarioId == usuarioId)
                .Select(dt => dt.Token)
                .ToListAsync(cancellationToken);

            if (tokens.Count == 0)
                return;

            var payload = new
            {
                to = tokens,
                title,
                body,
                data,
                sound = "default",
                priority = "high",
            };

            var response = await _httpClient.PostAsJsonAsync(ExpoPushUrl, payload, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ExpoPushResponse>(cancellationToken: cancellationToken);

            if (result?.Data is not null)
            {
                var invalidTokens = result.Data
                    .Where(d => d.Status == "error" && (
                        d.Details?.Error == "DeviceNotRegistered" ||
                        d.Details?.Error == "InvalidCredentials"))
                    .Select(d => d.Details?.ExpoToken)
                    .Where(t => t is not null)
                    .ToList();

                if (invalidTokens.Count > 0)
                {
                    var tokensToRemove = await _db.DeviceTokens
                        .Where(dt => invalidTokens.Contains(dt.Token))
                        .ToListAsync(cancellationToken);

                    _db.DeviceTokens.RemoveRange(tokensToRemove);
                    await _db.SaveChangesAsync(cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send push notification to user {UsuarioId}", usuarioId);
        }
    }

    private sealed class ExpoPushResponse
    {
        [JsonPropertyName("data")]
        public List<ExpoPushTicket>? Data { get; set; }
    }

    private sealed class ExpoPushTicket
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("details")]
        public ExpoPushDetails? Details { get; set; }
    }

    private sealed class ExpoPushDetails
    {
        [JsonPropertyName("error")]
        public string? Error { get; set; }

        [JsonPropertyName("expoToken")]
        public string? ExpoToken { get; set; }
    }
}
