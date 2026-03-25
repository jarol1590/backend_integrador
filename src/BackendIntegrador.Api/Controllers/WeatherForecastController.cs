using BackendIntegrador.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace BackendIntegrador.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly IWeatherForecastProvider _forecastProvider;
    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(
        IWeatherForecastProvider forecastProvider,
        ILogger<WeatherForecastController> logger)
    {
        _forecastProvider = forecastProvider;
        _logger = logger;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var forecasts = await _forecastProvider.GetForecastsAsync(cancellationToken);
        return Ok(forecasts);
    }
}
