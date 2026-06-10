namespace BackendIntegrador.Application.Dtos;

public record ClimaActualDto(
    DateOnly Fecha,
    decimal TempMedia,
    decimal? HumedadMedia,
    decimal? ThiMax,
    int DiasConsecutivosCalor);

public record FincaGemeloEstadoDto(
    int FincaId,
    string FincaNombre,
    DateTime? UltimaSyncUtc,
    string VersionMotor,
    string FuenteClima,
    string EstadoSync,
    int ScoreRiesgoGlobal,
    ClimaActualDto? ClimaActual,
    int AlertasActivas);

public record LecturaClimaticaDto(
    DateOnly Fecha,
    decimal TempMin,
    decimal TempMax,
    decimal TempMedia,
    decimal? HumedadMedia,
    decimal? PrecipitacionMm,
    decimal? ThiMax,
    int DiasConsecutivosCalor,
    string Fuente);

public record PrediccionGemeloDto(
    string TipoPrediccion,
    int HorizonteDias,
    decimal Valor,
    decimal Confianza,
    string? Unidad,
    DateTime GeneradaUtc,
    string? DetalleJson);

public record AlertaGemeloDto(
    int AlertaId,
    int FincaId,
    string TipoAlerta,
    string Severidad,
    string Titulo,
    string Mensaje,
    string? Recomendacion,
    DateTime CreadaUtc,
    DateTime? ExpiraUtc,
    bool Leida);

public record SincronizarGemeloResultDto(
    int FincaId,
    DateTime SyncUtc,
    string EstadoSync,
    int LecturasActualizadas,
    int PrediccionesGeneradas,
    int AlertasNuevas,
    int ScoreRiesgoGlobal);

public record RiesgoRegionalFincaDto(
    int FincaId,
    string FincaNombre,
    string MunicipioNombre,
    int ScoreRiesgoGlobal,
    int AlertasActivas,
    decimal? TempMediaReciente,
    decimal? Latitud,
    decimal? Longitud);

public record CentroAcopioRiesgoRegionalDto(
    int CentroAcopioId,
    string CentroAcopioNombre,
    DateTime GeneradaUtc,
    IReadOnlyList<RiesgoRegionalFincaDto> Fincas,
    decimal ScoreRiesgoPromedio);

public record ClimateDailyReading(
    DateOnly Fecha,
    decimal TempMin,
    decimal TempMax,
    decimal TempMedia,
    decimal? HumedadMedia,
    decimal? PrecipitacionMm,
    string Fuente);

public record FincaProductionContext(
    int FincaId,
    int OrdenoCount,
    decimal VolumenPromedio14Dias,
    decimal? UltimoPh,
    decimal? PenultimoPh,
    IReadOnlyList<ClimateDailyReading> Lecturas);

public record PredictionResult(
    string TipoPrediccion,
    int HorizonteDias,
    decimal Valor,
    decimal Confianza,
    string? Unidad,
    string? DetalleJson);

public record AlertEvaluationResult(
    string TipoAlerta,
    string Severidad,
    string Titulo,
    string Mensaje,
    string? Recomendacion,
    DateTime? ExpiraUtc);
