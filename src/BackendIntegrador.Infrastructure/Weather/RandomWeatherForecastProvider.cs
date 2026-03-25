using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Domain.Entities;

namespace BackendIntegrador.Infrastructure.Weather;

public class RandomWeatherForecastProvider : IWeatherForecastProvider
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    public Task<IReadOnlyList<WeatherForecast>> GetForecastsAsync(CancellationToken cancellationToken = default)
    {
        var items = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        }).ToList();

        return Task.FromResult<IReadOnlyList<WeatherForecast>>(items);
    }
}
