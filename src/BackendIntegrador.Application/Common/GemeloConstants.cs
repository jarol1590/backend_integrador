namespace BackendIntegrador.Application.Common;

public static class GemeloSyncEstados
{
    public const string Ok = "ok";
    public const string Degradado = "degradado";
    public const string Error = "error";
    public const string Pendiente = "pendiente";
}

public static class GemeloPrediccionTipos
{
    public const string VolumenProduccion = "volumen_produccion";
    public const string RiesgoAcidificacion = "riesgo_acidificacion";
    public const string ScoreRiesgoGlobal = "score_riesgo_global";
}

public static class GemeloAlertaTipos
{
    public const string OlaCalorAcidificacion = "ola_calor_acidificacion";
    public const string CaidaVolumenEstresTermico = "caida_volumen_estres_termico";
    public const string SyncClimaFallida = "sync_clima_fallida";
}

public static class GemeloSeveridades
{
    public const string Baja = "baja";
    public const string Media = "media";
    public const string Alta = "alta";
    public const string Critica = "critica";
}

public static class GemeloCalidadParametros
{
    public const string Acidez = "Acidez";
}
