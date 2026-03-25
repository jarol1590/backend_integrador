using BackendIntegrador.Domain.Entities;

namespace BackendIntegrador.Application.Abstractions;

public interface IWeatherForecastProvider
{
    Task<IReadOnlyList<WeatherForecast>> GetForecastsAsync(CancellationToken cancellationToken = default);
}
