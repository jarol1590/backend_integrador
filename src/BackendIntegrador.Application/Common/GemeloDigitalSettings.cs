namespace BackendIntegrador.Application.Common;

public class OpenMeteoSettings
{
    public string HistoricalBaseUrl { get; set; } = "https://archive-api.open-meteo.com/v1/archive";
    public string ForecastBaseUrl { get; set; } = "https://api.open-meteo.com/v1/forecast";
    public int TimeoutSeconds { get; set; } = 10;
    public int HistoricalDaysDefault { get; set; } = 90;
    public int ForecastDaysDefault { get; set; } = 7;
    public decimal ThiThreshold { get; set; } = 72m;
    public int HeatWaveConsecutiveDays { get; set; } = 3;
}

public class GemeloDigitalSettings
{
    public string MotorVersion { get; set; } = "heuristic-v1";
    public int MinOrdenosForConfidence { get; set; } = 5;
    public int DefaultHorizonteDias { get; set; } = 7;
}
