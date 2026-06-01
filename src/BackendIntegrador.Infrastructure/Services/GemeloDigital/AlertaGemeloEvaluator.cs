using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Common;
using BackendIntegrador.Application.Dtos;

namespace BackendIntegrador.Infrastructure.Services.GemeloDigital;

public sealed class AlertaGemeloEvaluator : IAlertaGemeloEvaluator
{
    private readonly OpenMeteoSettings _openMeteo;

    public AlertaGemeloEvaluator(OpenMeteoSettings openMeteo)
    {
        _openMeteo = openMeteo;
    }

    public IReadOnlyList<AlertEvaluationResult> Evaluate(FincaProductionContext context, int scoreRiesgoGlobal)
    {
        var alertas = new List<AlertEvaluationResult>();
        var lecturas = context.Lecturas.OrderBy(l => l.Fecha).ToList();
        var thiSeries = lecturas
            .Select(l =>
            {
                var hum = l.HumedadMedia ?? 60m;
                return (l.Fecha, ThiMax: GemeloClimateMath.CalculateThi(l.TempMax, hum));
            })
            .ToList();

        var diasCalor = GemeloClimateMath.CountTrailingHotDays(thiSeries, _openMeteo.ThiThreshold);
        var phCayendo = context.UltimoPh is not null && context.PenultimoPh is not null && context.UltimoPh < context.PenultimoPh;

        if (diasCalor >= _openMeteo.HeatWaveConsecutiveDays && (phCayendo || context.UltimoPh is null))
        {
            alertas.Add(new AlertEvaluationResult(
                GemeloAlertaTipos.OlaCalorAcidificacion,
                diasCalor >= 5 ? GemeloSeveridades.Alta : GemeloSeveridades.Media,
                "Riesgo de acidificación por ola de calor",
                $"Se detectaron {diasCalor} días consecutivos con estrés térmico (THI ≥ {_openMeteo.ThiThreshold})." +
                (phCayendo ? " El pH reciente muestra tendencia a la baja." : ""),
                "Considere ordeñar en horas frescas, mejorar sombra/ventilación del ganado y revisar enfriamiento del tanque.",
                DateTime.UtcNow.AddDays(7)));
        }

        if (diasCalor >= 2 && context.VolumenPromedio14Dias > 0)
        {
            var reduccionEstimada = Math.Min(diasCalor * 4, 30);
            alertas.Add(new AlertEvaluationResult(
                GemeloAlertaTipos.CaidaVolumenEstresTermico,
                reduccionEstimada >= 20 ? GemeloSeveridades.Media : GemeloSeveridades.Baja,
                "Posible caída de producción por estrés térmico",
                $"El pronóstico climático sugiere una reducción estimada de hasta {reduccionEstimada}% en el volumen diario.",
                "Asegure disponibilidad de agua limpia y revise la dieta energética del hato.",
                DateTime.UtcNow.AddDays(5)));
        }

        if (scoreRiesgoGlobal >= 80 && alertas.Count == 0)
        {
            alertas.Add(new AlertEvaluationResult(
                GemeloAlertaTipos.OlaCalorAcidificacion,
                GemeloSeveridades.Media,
                "Riesgo climático elevado en la finca",
                $"El score de riesgo global es {scoreRiesgoGlobal}/100.",
                "Revise condiciones ambientales del ganado y programe análisis de calidad.",
                DateTime.UtcNow.AddDays(5)));
        }

        return alertas;
    }
}
