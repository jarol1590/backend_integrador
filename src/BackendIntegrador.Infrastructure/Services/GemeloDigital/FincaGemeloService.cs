using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Common;
using BackendIntegrador.Application.Dtos;
using BackendIntegrador.Domain.Entities;
using BackendIntegrador.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BackendIntegrador.Infrastructure.Services.GemeloDigital;

public sealed class FincaGemeloService : IFincaGemeloService
{
    private readonly AppDbContext _db;
    private readonly IClimateDataProvider _climate;
    private readonly IMilkQualityPredictor _predictor;
    private readonly IAlertaGemeloEvaluator _alertEvaluator;
    private readonly OpenMeteoSettings _openMeteo;
    private readonly GemeloDigitalSettings _gemelo;

    public FincaGemeloService(
        AppDbContext db,
        IClimateDataProvider climate,
        IMilkQualityPredictor predictor,
        IAlertaGemeloEvaluator alertEvaluator,
        OpenMeteoSettings openMeteo,
        GemeloDigitalSettings gemelo)
    {
        _db = db;
        _climate = climate;
        _predictor = predictor;
        _alertEvaluator = alertEvaluator;
        _openMeteo = openMeteo;
        _gemelo = gemelo;
    }

    public async Task<FincaGemeloEstadoDto> GetEstadoAsync(int fincaId, CancellationToken cancellationToken = default)
    {
        var finca = await GetFincaOrThrowAsync(fincaId, cancellationToken);
        var estado = await _db.FincasGemeloEstado.FirstOrDefaultAsync(e => e.FincaId == fincaId, cancellationToken);
        var ultimaLectura = await _db.LecturasClimaticas
            .Where(l => l.FincaId == fincaId)
            .OrderByDescending(l => l.Fecha)
            .FirstOrDefaultAsync(cancellationToken);

        var alertasActivas = await CountAlertasActivasAsync(fincaId, cancellationToken);

        return new FincaGemeloEstadoDto(
            fincaId,
            finca.Nombre,
            estado?.UltimaSyncUtc,
            estado?.VersionMotor ?? _gemelo.MotorVersion,
            estado?.FuenteClima ?? "open-meteo",
            estado?.EstadoSync ?? GemeloSyncEstados.Pendiente,
            estado?.ScoreRiesgoGlobal ?? 0,
            ultimaLectura is null ? null : MapClimaActual(ultimaLectura),
            alertasActivas);
    }

    public async Task<IReadOnlyList<LecturaClimaticaDto>> GetClimaAsync(
        int fincaId,
        DateOnly? desde,
        DateOnly? hasta,
        CancellationToken cancellationToken = default)
    {
        await GetFincaOrThrowAsync(fincaId, cancellationToken);

        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var inicio = desde ?? hoy.AddDays(-30);
        var fin = hasta ?? hoy;

        var lecturas = await _db.LecturasClimaticas
            .Where(l => l.FincaId == fincaId && l.Fecha >= inicio && l.Fecha <= fin)
            .OrderBy(l => l.Fecha)
            .ToListAsync(cancellationToken);

        return lecturas.Select(MapLectura).ToList();
    }

    public async Task<IReadOnlyList<PrediccionGemeloDto>> GetPrediccionesAsync(
        int fincaId,
        int? horizonteDias,
        CancellationToken cancellationToken = default)
    {
        await GetFincaOrThrowAsync(fincaId, cancellationToken);
        var horizonte = horizonteDias ?? _gemelo.DefaultHorizonteDias;

        var predicciones = await _db.PrediccionesGemelo
            .Where(p => p.FincaId == fincaId && p.HorizonteDias == horizonte)
            .OrderByDescending(p => p.GeneradaUtc)
            .ToListAsync(cancellationToken);

        return predicciones
            .GroupBy(p => p.TipoPrediccion)
            .Select(g => g.First())
            .Select(MapPrediccion)
            .ToList();
    }

    public async Task<IReadOnlyList<AlertaGemeloDto>> GetAlertasAsync(
        int fincaId,
        bool? activas,
        CancellationToken cancellationToken = default)
    {
        await GetFincaOrThrowAsync(fincaId, cancellationToken);
        var now = DateTime.UtcNow;

        var query = _db.AlertasGemelo.Where(a => a.FincaId == fincaId);

        if (activas == true)
        {
            query = query.Where(a => !a.Leida && (a.ExpiraUtc == null || a.ExpiraUtc > now));
        }

        var alertas = await query
            .OrderByDescending(a => a.CreadaUtc)
            .ToListAsync(cancellationToken);

        return alertas.Select(MapAlerta).ToList();
    }

    public async Task<SincronizarGemeloResultDto> SincronizarAsync(int fincaId, CancellationToken cancellationToken = default)
    {
        var finca = await GetFincaOrThrowAsync(fincaId, cancellationToken);

        if (finca.Latitud is null || finca.Longitud is null)
            throw new InvalidOperationException("La finca debe tener latitud y longitud para sincronizar el gemelo digital.");

        var now = DateTime.UtcNow;
        var hoy = DateOnly.FromDateTime(now);
        var desdeHistorico = hoy.AddDays(-_openMeteo.HistoricalDaysDefault);

        var estado = await _db.FincasGemeloEstado.FirstOrDefaultAsync(e => e.FincaId == fincaId, cancellationToken);
        if (estado is null)
        {
            estado = new FincaGemeloEstado
            {
                FincaId = fincaId,
                CreadoUtc = now,
                VersionMotor = _gemelo.MotorVersion,
                FuenteClima = "open-meteo",
                EstadoSync = GemeloSyncEstados.Pendiente
            };
            _db.FincasGemeloEstado.Add(estado);
        }

        var lecturasActualizadas = 0;
        try
        {
            var historical = await _climate.GetHistoricalAsync(
                finca.Latitud.Value, finca.Longitud.Value, desdeHistorico, hoy, cancellationToken);
            var forecast = await _climate.GetForecastAsync(
                finca.Latitud.Value, finca.Longitud.Value, _openMeteo.ForecastDaysDefault, cancellationToken);

            var allReadings = historical.Concat(forecast)
                .GroupBy(r => r.Fecha)
                .Select(g => g.Last())
                .OrderBy(r => r.Fecha)
                .ToList();

            lecturasActualizadas = await UpsertLecturasAsync(fincaId, allReadings, cancellationToken);

            var context = await BuildProductionContextAsync(fincaId, allReadings, cancellationToken);
            var horizonte = _gemelo.DefaultHorizonteDias;
            var predictions = _predictor.Predict(context, horizonte);
            var prediccionesGeneradas = await UpsertPrediccionesAsync(fincaId, predictions, cancellationToken);

            var scoreGlobal = (int)(predictions
                .FirstOrDefault(p => p.TipoPrediccion == GemeloPrediccionTipos.ScoreRiesgoGlobal)?.Valor ?? 0m);

            var alertCandidates = _alertEvaluator.Evaluate(context, scoreGlobal);
            var alertasNuevas = await InsertAlertasAsync(fincaId, alertCandidates, cancellationToken);

            estado.UltimaSyncUtc = now;
            estado.ActualizadoUtc = now;
            estado.VersionMotor = _gemelo.MotorVersion;
            estado.FuenteClima = "open-meteo";
            estado.ScoreRiesgoGlobal = scoreGlobal;
            estado.EstadoSync = GemeloSyncEstados.Ok;
            estado.UltimoError = null;

            await _db.SaveChangesAsync(cancellationToken);

            return new SincronizarGemeloResultDto(
                fincaId, now, GemeloSyncEstados.Ok, lecturasActualizadas, prediccionesGeneradas, alertasNuevas, scoreGlobal);
        }
        catch (Exception ex)
        {
            estado.ActualizadoUtc = now;
            estado.EstadoSync = GemeloSyncEstados.Error;
            estado.UltimoError = ex.Message.Length > 1024 ? ex.Message[..1024] : ex.Message;
            await _db.SaveChangesAsync(cancellationToken);
            throw;
        }
    }

    public async Task MarcarAlertaLeidaAsync(int fincaId, int alertaId, CancellationToken cancellationToken = default)
    {
        await GetFincaOrThrowAsync(fincaId, cancellationToken);

        var alerta = await _db.AlertasGemelo
            .FirstOrDefaultAsync(a => a.AlertaId == alertaId && a.FincaId == fincaId, cancellationToken);

        if (alerta is null)
            throw new KeyNotFoundException("Alerta no encontrada.");

        alerta.Leida = true;
        alerta.LeidaUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<Finca> GetFincaOrThrowAsync(int fincaId, CancellationToken cancellationToken)
    {
        var finca = await _db.Fincas.FirstOrDefaultAsync(f => f.FincaId == fincaId, cancellationToken);
        if (finca is null)
            throw new KeyNotFoundException("Finca no encontrada.");
        return finca;
    }

    private async Task<int> UpsertLecturasAsync(
        int fincaId,
        IReadOnlyList<ClimateDailyReading> readings,
        CancellationToken cancellationToken)
    {
        var existing = await _db.LecturasClimaticas
            .Where(l => l.FincaId == fincaId)
            .ToDictionaryAsync(l => l.Fecha, cancellationToken);

        var thiByDate = new List<(DateOnly Fecha, decimal ThiMax)>();
        var count = 0;

        foreach (var reading in readings.OrderBy(r => r.Fecha))
        {
            var hum = reading.HumedadMedia ?? 60m;
            var thi = GemeloClimateMath.CalculateThi(reading.TempMax, hum);
            thiByDate.Add((reading.Fecha, thi));
        }

        foreach (var reading in readings.OrderBy(r => r.Fecha))
        {
            var hum = reading.HumedadMedia ?? 60m;
            var thi = GemeloClimateMath.CalculateThi(reading.TempMax, hum);
            var diasCalor = GemeloClimateMath.CountTrailingHotDays(
                thiByDate.Where(x => x.Fecha <= reading.Fecha).ToList(),
                _openMeteo.ThiThreshold);

            if (existing.TryGetValue(reading.Fecha, out var entity))
            {
                entity.TempMin = reading.TempMin;
                entity.TempMax = reading.TempMax;
                entity.TempMedia = reading.TempMedia;
                entity.HumedadMedia = reading.HumedadMedia;
                entity.PrecipitacionMm = reading.PrecipitacionMm;
                entity.ThiMax = thi;
                entity.DiasConsecutivosCalor = diasCalor;
                entity.Fuente = reading.Fuente;
            }
            else
            {
                _db.LecturasClimaticas.Add(new LecturaClimatica
                {
                    FincaId = fincaId,
                    Fecha = reading.Fecha,
                    TempMin = reading.TempMin,
                    TempMax = reading.TempMax,
                    TempMedia = reading.TempMedia,
                    HumedadMedia = reading.HumedadMedia,
                    PrecipitacionMm = reading.PrecipitacionMm,
                    ThiMax = thi,
                    DiasConsecutivosCalor = diasCalor,
                    Fuente = reading.Fuente
                });
            }

            count++;
        }

        return count;
    }

    private async Task<int> UpsertPrediccionesAsync(
        int fincaId,
        IReadOnlyList<PredictionResult> predictions,
        CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow.AddHours(-24);
        var stale = await _db.PrediccionesGemelo
            .Where(p => p.FincaId == fincaId && p.GeneradaUtc >= cutoff)
            .ToListAsync(cancellationToken);

        foreach (var old in stale)
            _db.PrediccionesGemelo.Remove(old);

        var now = DateTime.UtcNow;
        foreach (var p in predictions)
        {
            _db.PrediccionesGemelo.Add(new PrediccionGemelo
            {
                FincaId = fincaId,
                GeneradaUtc = now,
                HorizonteDias = p.HorizonteDias,
                TipoPrediccion = p.TipoPrediccion,
                Valor = p.Valor,
                Confianza = p.Confianza,
                Unidad = p.Unidad,
                DetalleJson = p.DetalleJson
            });
        }

        return predictions.Count;
    }

    private async Task<int> InsertAlertasAsync(
        int fincaId,
        IReadOnlyList<AlertEvaluationResult> candidates,
        CancellationToken cancellationToken)
    {
        var since = DateTime.UtcNow.AddHours(-24);
        var existingTypes = await _db.AlertasGemelo
            .Where(a => a.FincaId == fincaId && a.CreadaUtc >= since && !a.Leida)
            .Select(a => a.TipoAlerta)
            .ToListAsync(cancellationToken);

        var nuevas = 0;
        var now = DateTime.UtcNow;

        foreach (var candidate in candidates)
        {
            if (existingTypes.Contains(candidate.TipoAlerta))
                continue;

            _db.AlertasGemelo.Add(new AlertaGemelo
            {
                FincaId = fincaId,
                TipoAlerta = candidate.TipoAlerta,
                Severidad = candidate.Severidad,
                Titulo = candidate.Titulo,
                Mensaje = candidate.Mensaje,
                Recomendacion = candidate.Recomendacion,
                CreadaUtc = now,
                ExpiraUtc = candidate.ExpiraUtc,
                Leida = false
            });
            nuevas++;
        }

        return nuevas;
    }

    private async Task<FincaProductionContext> BuildProductionContextAsync(
        int fincaId,
        IReadOnlyList<ClimateDailyReading> lecturas,
        CancellationToken cancellationToken)
    {
        var desde = DateTime.UtcNow.AddDays(-14);
        var ordenos = await _db.Ordenos
            .Where(o => o.FincaId == fincaId && o.FechaHoraInicio >= desde)
            .ToListAsync(cancellationToken);

        var volumenPromedio = ordenos.Count > 0
            ? ordenos.Average(o => o.VolumenLitros)
            : 0m;

        var phValues = await _db.ResultadosParametro
            .Where(r => r.Parametro.Nombre == GemeloCalidadParametros.Acidez &&
                        r.Analisis.Muestra.Lote.Ordeno.FincaId == fincaId)
            .OrderByDescending(r => r.Analisis.FechaHoraAnalisis)
            .Select(r => r.ValorResultado)
            .Take(2)
            .ToListAsync(cancellationToken);

        return new FincaProductionContext(
            fincaId,
            ordenos.Count,
            Math.Round(volumenPromedio, 2),
            phValues.Count > 0 ? phValues[0] : null,
            phValues.Count > 1 ? phValues[1] : null,
            lecturas);
    }

    private async Task<int> CountAlertasActivasAsync(int fincaId, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        return await _db.AlertasGemelo
            .CountAsync(a => a.FincaId == fincaId && !a.Leida && (a.ExpiraUtc == null || a.ExpiraUtc > now), cancellationToken);
    }

    private static ClimaActualDto MapClimaActual(LecturaClimatica l) =>
        new(l.Fecha, l.TempMedia, l.HumedadMedia, l.ThiMax, l.DiasConsecutivosCalor);

    private static LecturaClimaticaDto MapLectura(LecturaClimatica l) =>
        new(l.Fecha, l.TempMin, l.TempMax, l.TempMedia, l.HumedadMedia, l.PrecipitacionMm, l.ThiMax, l.DiasConsecutivosCalor, l.Fuente);

    private static PrediccionGemeloDto MapPrediccion(PrediccionGemelo p) =>
        new(p.TipoPrediccion, p.HorizonteDias, p.Valor, p.Confianza, p.Unidad, p.GeneradaUtc, p.DetalleJson);

    private static AlertaGemeloDto MapAlerta(AlertaGemelo a) =>
        new(a.AlertaId, a.FincaId, a.TipoAlerta, a.Severidad, a.Titulo, a.Mensaje, a.Recomendacion, a.CreadaUtc, a.ExpiraUtc, a.Leida);
}
