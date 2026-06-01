using System;
using System.Collections.Generic;
using System.Linq;
using BackendIntegrador.Application.Common;
using BackendIntegrador.Application.Dtos;
using BackendIntegrador.Infrastructure.Services.GemeloDigital;
using FluentAssertions;
using Xunit;

namespace BackendIntegrador.Tests;

public class HeuristicMilkQualityPredictorTests
{
    private readonly HeuristicMilkQualityPredictor _predictor = new(
        new OpenMeteoSettings { ThiThreshold = 72, HeatWaveConsecutiveDays = 3 },
        new GemeloDigitalSettings { MinOrdenosForConfidence = 5, DefaultHorizonteDias = 7 });

    [Fact]
    public void Predict_HotDays_ReducesProjectedVolume()
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var lecturas = Enumerable.Range(0, 7)
            .Select(i => new ClimateDailyReading(
                hoy.AddDays(-i),
                30m, 38m, 34m, 70m, 0m,
                "test"))
            .ToList();

        var context = new FincaProductionContext(1, 10, 100m, 6.7m, 6.8m, lecturas);
        var results = _predictor.Predict(context, 7);

        var volumen = results.First(r => r.TipoPrediccion == GemeloPrediccionTipos.VolumenProduccion);
        volumen.Valor.Should().BeLessThan(100m);
    }

    [Fact]
    public void Predict_FallingPh_IncreasesAcidificationRisk()
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var lecturas = new List<ClimateDailyReading>
        {
            new(hoy, 28m, 36m, 32m, 65m, 0m, "test")
        };

        var withPhDrop = new FincaProductionContext(1, 8, 80m, 6.4m, 6.7m, lecturas);
        var withoutPhDrop = new FincaProductionContext(1, 8, 80m, 6.7m, 6.7m, lecturas);

        var riskWith = _predictor.Predict(withPhDrop, 7)
            .First(r => r.TipoPrediccion == GemeloPrediccionTipos.RiesgoAcidificacion).Valor;
        var riskWithout = _predictor.Predict(withoutPhDrop, 7)
            .First(r => r.TipoPrediccion == GemeloPrediccionTipos.RiesgoAcidificacion).Valor;

        riskWith.Should().BeGreaterThan(riskWithout);
    }
}

public class AlertaGemeloEvaluatorTests
{
    private readonly AlertaGemeloEvaluator _evaluator = new(
        new OpenMeteoSettings { ThiThreshold = 72, HeatWaveConsecutiveDays = 3 });

    [Fact]
    public void Evaluate_ProlongedHeat_GeneratesAcidificationAlert()
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var lecturas = Enumerable.Range(0, 4)
            .Select(i => new ClimateDailyReading(
                hoy.AddDays(-i),
                30m, 40m, 35m, 60m, 0m,
                "test"))
            .ToList();

        var context = new FincaProductionContext(1, 3, 50m, 6.5m, 6.6m, lecturas);
        var alertas = _evaluator.Evaluate(context, scoreRiesgoGlobal: 70);

        alertas.Should().Contain(a => a.TipoAlerta == GemeloAlertaTipos.OlaCalorAcidificacion);
    }

    [Fact]
    public void Evaluate_MildWeather_NoCriticalAlerts()
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var lecturas = new List<ClimateDailyReading>
        {
            new(hoy, 15m, 22m, 18m, 55m, 5m, "test")
        };

        var context = new FincaProductionContext(1, 2, 50m, null, null, lecturas);
        var alertas = _evaluator.Evaluate(context, scoreRiesgoGlobal: 10);

        alertas.Should().NotContain(a => a.Severidad == GemeloSeveridades.Alta);
    }
}

public class GemeloClimateMathTests
{
    [Fact]
    public void CountTrailingHotDays_CountsFromMostRecent()
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var series = new List<(DateOnly, decimal)>
        {
            (hoy.AddDays(-3), 70m),
            (hoy.AddDays(-2), 75m),
            (hoy.AddDays(-1), 78m),
            (hoy, 80m)
        };

        GemeloClimateMath.CountTrailingHotDays(series, 72).Should().Be(3);
    }
}
