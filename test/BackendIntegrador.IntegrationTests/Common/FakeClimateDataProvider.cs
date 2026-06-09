using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;

namespace BackendIntegrador.IntegrationTests.Common;

public sealed class FakeClimateDataProvider : IClimateDataProvider
{
    public Task<IReadOnlyList<ClimateDailyReading>> GetHistoricalAsync(
        decimal latitud,
        decimal longitud,
        DateOnly desde,
        DateOnly hasta,
        CancellationToken cancellationToken = default)
    {
        var readings = new List<ClimateDailyReading>();
        for (var d = desde; d <= hasta; d = d.AddDays(1))
        {
            readings.Add(new ClimateDailyReading(
                d,
                22m, 36m, 29m, 65m, 2m,
                "fake-historical"));
        }

        return Task.FromResult<IReadOnlyList<ClimateDailyReading>>(readings);
    }

    public Task<IReadOnlyList<ClimateDailyReading>> GetForecastAsync(
        decimal latitud,
        decimal longitud,
        int dias,
        CancellationToken cancellationToken = default)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var readings = Enumerable.Range(0, dias)
            .Select(i => new ClimateDailyReading(
                hoy.AddDays(i),
                24m, 37m, 30m, 60m, 0m,
                "fake-forecast"))
            .ToList();

        return Task.FromResult<IReadOnlyList<ClimateDailyReading>>(readings);
    }
}
