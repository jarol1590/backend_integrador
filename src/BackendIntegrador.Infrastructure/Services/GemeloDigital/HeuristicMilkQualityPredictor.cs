using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Common;
using BackendIntegrador.Application.Dtos;

namespace BackendIntegrador.Infrastructure.Services.GemeloDigital;

public sealed class HeuristicMilkQualityPredictor : IMilkQualityPredictor
{
    private readonly OpenMeteoSettings _openMeteo;
    private readonly GemeloDigitalSettings _gemelo;

    public HeuristicMilkQualityPredictor(OpenMeteoSettings openMeteo, GemeloDigitalSettings gemelo)
    {
        _openMeteo = openMeteo;
        _gemelo = gemelo;
    }

    public IReadOnlyList<PredictionResult> Predict(FincaProductionContext context, int horizonteDias)
    {
        var lecturas = context.Lecturas.OrderBy(l => l.Fecha).ToList();
        var thiSeries = lecturas
            .Select(l =>
            {
                var hum = l.HumedadMedia ?? 60m;
                var thi = GemeloClimateMath.CalculateThi(l.TempMax, hum);
                return (l.Fecha, ThiMax: thi);
            })
            .ToList();

        var diasCalor = GemeloClimateMath.CountTrailingHotDays(thiSeries, _openMeteo.ThiThreshold);
        var forecastHotDays = thiSeries
            .Where(x => x.Fecha >= DateOnly.FromDateTime(DateTime.UtcNow))
            .Count(x => x.ThiMax >= _openMeteo.ThiThreshold);

        var baseVolumen = context.VolumenPromedio14Dias;
        var factorCalor = 1m - (0.01m * Math.Min(diasCalor + forecastHotDays, 15));
        var volumenProyectado = Math.Round(baseVolumen * factorCalor, 2);

        var confianzaBase = Math.Min(1m, context.OrdenoCount / (decimal)Math.Max(1, _gemelo.MinOrdenosForConfidence));
        var confianzaVolumen = Math.Round(Math.Max(0.3m, confianzaBase), 3);

        var riesgoAcidificacion = CalculateAcidificationRisk(diasCalor, context.UltimoPh, context.PenultimoPh);
        var scoreGlobal = Math.Clamp((int)Math.Round((riesgoAcidificacion * 0.6m) + (diasCalor * 8m)), 0, 100);

        return new List<PredictionResult>
        {
            new(
                GemeloPrediccionTipos.VolumenProduccion,
                horizonteDias,
                volumenProyectado,
                confianzaVolumen,
                "L/dia",
                $"{{\"diasCalor\":{diasCalor},\"factorCalor\":{factorCalor}}}"),
            new(
                GemeloPrediccionTipos.RiesgoAcidificacion,
                horizonteDias,
                riesgoAcidificacion,
                context.UltimoPh.HasValue ? 0.7m : 0.4m,
                "score",
                null),
            new(
                GemeloPrediccionTipos.ScoreRiesgoGlobal,
                horizonteDias,
                scoreGlobal,
                0.65m,
                "score",
                null)
        };
    }

    private static decimal CalculateAcidificationRisk(int diasCalor, decimal? ultimoPh, decimal? penultimoPh)
    {
        var riesgo = Math.Min(diasCalor * 15m, 60m);
        if (ultimoPh is not null && penultimoPh is not null && ultimoPh < penultimoPh)
            riesgo += 25m;
        else if (ultimoPh is not null && ultimoPh < 6.5m)
            riesgo += 15m;

        return Math.Clamp(Math.Round(riesgo, 2), 0m, 100m);
    }
}
