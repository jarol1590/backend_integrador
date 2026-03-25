using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Infrastructure.Weather;
using Microsoft.Extensions.DependencyInjection;

namespace BackendIntegrador.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IWeatherForecastProvider, RandomWeatherForecastProvider>();
        return services;
    }
}
