using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Common;
using BackendIntegrador.Application.Dtos;

namespace BackendIntegrador.Infrastructure.Services.GemeloDigital;

public sealed class OpenMeteoClimateProvider : IClimateDataProvider
{
    private readonly HttpClient _httpClient;
    private readonly OpenMeteoSettings _settings;

    public OpenMeteoClimateProvider(HttpClient httpClient, OpenMeteoSettings settings)
    {
        _httpClient = httpClient;
        _settings = settings;
        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
    }

    public Task<IReadOnlyList<ClimateDailyReading>> GetHistoricalAsync(
        decimal latitud,
        decimal longitud,
        DateOnly desde,
        DateOnly hasta,
        CancellationToken cancellationToken = default)
        => FetchDailyAsync(
            _settings.HistoricalBaseUrl,
            latitud,
            longitud,
            desde,
            hasta,
            "open-meteo-historical",
            cancellationToken);

    public async Task<IReadOnlyList<ClimateDailyReading>> GetForecastAsync(
        decimal latitud,
        decimal longitud,
        int dias,
        CancellationToken cancellationToken = default)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var hasta = hoy.AddDays(Math.Max(1, dias) - 1);
        return await FetchDailyAsync(
            _settings.ForecastBaseUrl,
            latitud,
            longitud,
            hoy,
            hasta,
            "open-meteo-forecast",
            cancellationToken);
    }

    private async Task<IReadOnlyList<ClimateDailyReading>> FetchDailyAsync(
        string baseUrl,
        decimal latitud,
        decimal longitud,
        DateOnly desde,
        DateOnly hasta,
        string fuente,
        CancellationToken cancellationToken)
    {
        var url =
            $"{baseUrl}?latitude={latitud.ToString(CultureInfo.InvariantCulture)}" +
            $"&longitude={longitud.ToString(CultureInfo.InvariantCulture)}" +
            $"&start_date={desde:yyyy-MM-dd}&end_date={hasta:yyyy-MM-dd}" +
            "&daily=temperature_2m_max,temperature_2m_min,temperature_2m_mean,precipitation_sum,relative_humidity_2m_mean" +
            "&timezone=UTC";

        var response = await _httpClient.GetFromJsonAsync<OpenMeteoDailyResponse>(url, cancellationToken);
        if (response?.Daily?.Time is null || response.Daily.Time.Count == 0)
            return Array.Empty<ClimateDailyReading>();

        var readings = new List<ClimateDailyReading>();
        for (var i = 0; i < response.Daily.Time.Count; i++)
        {
            if (!DateOnly.TryParse(response.Daily.Time[i], out var fecha))
                continue;

            var tempMinRaw = GetDecimal(response.Daily.TemperatureMin, i);
            var tempMaxRaw = GetDecimal(response.Daily.TemperatureMax, i);
            var tempMediaRaw = GetDecimal(response.Daily.TemperatureMean, i);
            var tempMedia = tempMediaRaw ?? ((tempMinRaw ?? 20m) + (tempMaxRaw ?? 20m)) / 2m;
            var tempMin = tempMinRaw ?? tempMedia;
            var tempMax = tempMaxRaw ?? tempMedia;
            var humedad = GetDecimal(response.Daily.HumidityMean, i);
            var precip = GetDecimal(response.Daily.PrecipitationSum, i);

            readings.Add(new ClimateDailyReading(
                fecha,
                tempMin,
                tempMax,
                tempMedia,
                humedad,
                precip,
                fuente));
        }

        return readings;
    }

    private static decimal? GetDecimal(IReadOnlyList<double?>? values, int index)
    {
        if (values is null || index >= values.Count || values[index] is null)
            return null;
        return (decimal)values[index]!.Value;
    }

    private sealed class OpenMeteoDailyResponse
    {
        [JsonPropertyName("daily")]
        public OpenMeteoDaily? Daily { get; set; }
    }

    private sealed class OpenMeteoDaily
    {
        [JsonPropertyName("time")]
        public List<string> Time { get; set; } = new();

        [JsonPropertyName("temperature_2m_min")]
        public List<double?>? TemperatureMin { get; set; }

        [JsonPropertyName("temperature_2m_max")]
        public List<double?>? TemperatureMax { get; set; }

        [JsonPropertyName("temperature_2m_mean")]
        public List<double?>? TemperatureMean { get; set; }

        [JsonPropertyName("relative_humidity_2m_mean")]
        public List<double?>? HumidityMean { get; set; }

        [JsonPropertyName("precipitation_sum")]
        public List<double?>? PrecipitationSum { get; set; }
    }
}
