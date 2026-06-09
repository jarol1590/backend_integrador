using BackendIntegrador.Application.Dtos;

namespace BackendIntegrador.Application.Abstractions;

public interface IClimateDataProvider
{
    Task<IReadOnlyList<ClimateDailyReading>> GetHistoricalAsync(
        decimal latitud,
        decimal longitud,
        DateOnly desde,
        DateOnly hasta,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClimateDailyReading>> GetForecastAsync(
        decimal latitud,
        decimal longitud,
        int dias,
        CancellationToken cancellationToken = default);
}

public interface IMilkQualityPredictor
{
    IReadOnlyList<PredictionResult> Predict(FincaProductionContext context, int horizonteDias);
}

public interface IAlertaGemeloEvaluator
{
    IReadOnlyList<AlertEvaluationResult> Evaluate(FincaProductionContext context, int scoreRiesgoGlobal);
}

public interface IFincaGemeloAuthorizationService
{
    Task EnsureCanAccessFincaAsync(int usuarioId, int fincaId, CancellationToken cancellationToken = default);
    Task EnsureCanAccessCentroAsync(int usuarioId, int centroAcopioId, CancellationToken cancellationToken = default);
    Task<bool> IsAdministradorAsync(int usuarioId, CancellationToken cancellationToken = default);
}

public interface IFincaGemeloService
{
    Task<FincaGemeloEstadoDto> GetEstadoAsync(int fincaId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LecturaClimaticaDto>> GetClimaAsync(
        int fincaId,
        DateOnly? desde,
        DateOnly? hasta,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PrediccionGemeloDto>> GetPrediccionesAsync(
        int fincaId,
        int? horizonteDias,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AlertaGemeloDto>> GetAlertasAsync(
        int fincaId,
        bool? activas,
        CancellationToken cancellationToken = default);
    Task<SincronizarGemeloResultDto> SincronizarAsync(int fincaId, CancellationToken cancellationToken = default);
    Task MarcarAlertaLeidaAsync(int fincaId, int alertaId, CancellationToken cancellationToken = default);
}

public interface ICentroAcopioGemeloService
{
    Task<CentroAcopioRiesgoRegionalDto> GetRiesgoRegionalAsync(
        int centroAcopioId,
        CancellationToken cancellationToken = default);
}
