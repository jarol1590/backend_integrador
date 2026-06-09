using BackendIntegrador.Application.Common;
using BackendIntegrador.Domain.Entities;
using BackendIntegrador.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BackendIntegrador.Infrastructure.Services.Seeding;

public sealed class DatabaseSeeder
{
    private const string DefaultPassword = "Secret123!";

    private readonly AppDbContext _db;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(AppDbContext db, ILogger<DatabaseSeeder> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await LogCurrentSummaryAsync("Antes de seed", cancellationToken);

        // 1) Roles canónicos
        var rolAdmin = await EnsureRolAsync(UsuarioRoleTypes.RolNombreAdministrador, "Administrador global del sistema", cancellationToken);
        var rolCentro = await EnsureRolAsync(UsuarioRoleTypes.RolNombreCentroAcopio, "Responsable de centro de acopio", cancellationToken);
        var rolProductor = await EnsureRolAsync(UsuarioRoleTypes.RolNombreProductor, "Productor lácteo", cancellationToken);
        var rolTrabajador = await EnsureRolAsync(UsuarioRoleTypes.RolNombreTrabajadorCentroAcopio, "Trabajador operativo del centro", cancellationToken);

        // 2) Geografía básica + centro de acopio
        var depId = await EnsureDepartamentoAsync("Caldas", cancellationToken);
        var munId = await EnsureMunicipioAsync("Manizales", depId, cancellationToken);
        var centroId = await EnsureCentroAcopioAsync("Acopio Norte", "Calle 10 #20-30", 5.068900m, -75.517400m, munId, cancellationToken);

        // 3) Tipo de documento
        var tipoDocId = await EnsureTipoDocumentoAsync("Cédula de ciudadanía", "Documento de identidad", cancellationToken);

        // 4) Usuarios demo (4 roles) + asignación
        var adminUserId = await EnsureUsuarioAsync("admin@example.com", null, cancellationToken);
        await EnsureUsuarioRolAsync(adminUserId, rolAdmin.RolId, cancellationToken);

        var centroUserId = await EnsureUsuarioAsync("centro@example.com", centroId, cancellationToken);
        await EnsureUsuarioRolAsync(centroUserId, rolCentro.RolId, cancellationToken);

        var trabajadorUserId = await EnsureUsuarioAsync("trabajador@example.com", centroId, cancellationToken);
        await EnsureUsuarioRolAsync(trabajadorUserId, rolTrabajador.RolId, cancellationToken);

        var productorUserId = await EnsureUsuarioAsync("productor@example.com", null, cancellationToken);
        await EnsureUsuarioRolAsync(productorUserId, rolProductor.RolId, cancellationToken);

        // 5) Productor + finca con GPS (necesario para gemelo)
        var productorId = await EnsureProductorAsync(productorUserId, tipoDocId, "98765432", "María López", "3109876543", cancellationToken);
        var fincaId = await EnsureFincaAsync(
            productorId,
            munId,
            nombre: "Finca El Roble",
            direccion: "Vereda La Palma",
            latitud: 5.070000m,
            longitud: -75.520000m,
            cancellationToken);

        // 6) Datos transaccionales mínimos para gemelo + vista regional de centro:
        // - Ordeños recientes (volumen)
        await EnsureOrdenosRecientesAsync(fincaId, days: 14, cancellationToken);

        // - Transporte + lote al centro para que el centro pueda ver riesgo regional
        var transporteId = await EnsureTransporteAsync("ABC123", cancellationToken);
        var ordenoId = await GetUltimoOrdenoIdAsync(fincaId, cancellationToken);
        _ = await EnsureLoteAsync(ordenoId, centroId, transporteId, cancellationToken);

        // 7) Parámetros de calidad + una muestra/análisis con Acidez (pH) con tendencia a la baja
        var parametroAcidezId = await EnsureParametroCalidadAsync("Acidez", "pH", 4.5m, 7.5m, cancellationToken);
        await EnsureAnalisisAcidezAsync(fincaId, centroId, trabajadorUserId, parametroAcidezId, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        await LogCurrentSummaryAsync("Después de seed", cancellationToken);

        _logger.LogInformation("Seed completado. Credenciales demo: admin/centro/trabajador/productor con password {Password}", DefaultPassword);
    }

    private async Task LogCurrentSummaryAsync(string title, CancellationToken cancellationToken)
    {
        var summary = new
        {
            Roles = await _db.Roles.CountAsync(cancellationToken),
            Usuarios = await _db.Usuarios.CountAsync(cancellationToken),
            UsuarioRoles = await _db.UsuarioRoles.CountAsync(cancellationToken),
            Departamentos = await _db.Departamentos.CountAsync(cancellationToken),
            Municipios = await _db.Municipios.CountAsync(cancellationToken),
            CentrosAcopio = await _db.CentrosAcopio.CountAsync(cancellationToken),
            TiposDocumento = await _db.TiposDocumento.CountAsync(cancellationToken),
            Productores = await _db.Productores.CountAsync(cancellationToken),
            Fincas = await _db.Fincas.CountAsync(cancellationToken),
            Ordenos = await _db.Ordenos.CountAsync(cancellationToken),
            Lotes = await _db.Lotes.CountAsync(cancellationToken),
            Muestras = await _db.Muestras.CountAsync(cancellationToken),
            Analisis = await _db.AnalisisCalidad.CountAsync(cancellationToken),
            Parametros = await _db.ParametrosCalidad.CountAsync(cancellationToken),
            Resultados = await _db.ResultadosParametro.CountAsync(cancellationToken),
            LecturasClima = await _db.LecturasClimaticas.CountAsync(cancellationToken),
            Predicciones = await _db.PrediccionesGemelo.CountAsync(cancellationToken),
            AlertasGemelo = await _db.AlertasGemelo.CountAsync(cancellationToken),
        };

        _logger.LogInformation("{Title}: {@Summary}", title, summary);
    }

    private async Task<Rol> EnsureRolAsync(string nombre, string? descripcion, CancellationToken cancellationToken)
    {
        var rol = await _db.Roles.FirstOrDefaultAsync(r => r.Nombre == nombre, cancellationToken);
        if (rol is not null)
            return rol;

        rol = new Rol { Nombre = nombre, Descripcion = descripcion };
        _db.Roles.Add(rol);
        await _db.SaveChangesAsync(cancellationToken);
        return rol;
    }

    private async Task<int> EnsureDepartamentoAsync(string nombre, CancellationToken cancellationToken)
    {
        var dep = await _db.Departamentos.FirstOrDefaultAsync(d => d.Nombre == nombre, cancellationToken);
        if (dep is not null)
            return dep.DepartamentoId;

        dep = new Departamento { Nombre = nombre };
        _db.Departamentos.Add(dep);
        await _db.SaveChangesAsync(cancellationToken);
        return dep.DepartamentoId;
    }

    private async Task<int> EnsureMunicipioAsync(string nombre, int departamentoId, CancellationToken cancellationToken)
    {
        var mun = await _db.Municipios.FirstOrDefaultAsync(m => m.Nombre == nombre && m.DepartamentoId == departamentoId, cancellationToken);
        if (mun is not null)
            return mun.MunicipioId;

        mun = new Municipio { Nombre = nombre, DepartamentoId = departamentoId };
        _db.Municipios.Add(mun);
        await _db.SaveChangesAsync(cancellationToken);
        return mun.MunicipioId;
    }

    private async Task<int> EnsureCentroAcopioAsync(
        string nombre,
        string direccion,
        decimal latitud,
        decimal longitud,
        int municipioId,
        CancellationToken cancellationToken)
    {
        var centro = await _db.CentrosAcopio.FirstOrDefaultAsync(c => c.Nombre == nombre, cancellationToken);
        if (centro is not null)
            return centro.CentroAcopioId;

        centro = new CentroAcopio
        {
            Nombre = nombre,
            Direccion = direccion,
            Latitud = latitud,
            Longitud = longitud,
            MunicipioId = municipioId
        };
        _db.CentrosAcopio.Add(centro);
        await _db.SaveChangesAsync(cancellationToken);
        return centro.CentroAcopioId;
    }

    private async Task<int> EnsureTipoDocumentoAsync(string nombre, string? descripcion, CancellationToken cancellationToken)
    {
        var tipo = await _db.TiposDocumento.FirstOrDefaultAsync(t => t.Nombre == nombre, cancellationToken);
        if (tipo is not null)
            return tipo.TipoDocumentoId;

        tipo = new TipoDocumento { Nombre = nombre, Descripcion = descripcion };
        _db.TiposDocumento.Add(tipo);
        await _db.SaveChangesAsync(cancellationToken);
        return tipo.TipoDocumentoId;
    }

    private async Task<int> EnsureUsuarioAsync(string email, int? centroAcopioId, CancellationToken cancellationToken)
    {
        var u = await _db.Usuarios.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
        if (u is not null)
        {
            // Asegurar estado activo y centro si aplica
            if (u.Estado != "activo")
                u.Estado = "activo";
            if (centroAcopioId.HasValue)
                u.CentroAcopioId = centroAcopioId;

            if (string.IsNullOrWhiteSpace(u.PasswordHash))
                u.PasswordHash = BCrypt.Net.BCrypt.HashPassword(DefaultPassword);

            await _db.SaveChangesAsync(cancellationToken);
            return u.UsuarioId;
        }

        u = new Usuario
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(DefaultPassword),
            Estado = "activo",
            FechaCreacion = DateTime.UtcNow,
            CentroAcopioId = centroAcopioId
        };
        _db.Usuarios.Add(u);
        await _db.SaveChangesAsync(cancellationToken);
        return u.UsuarioId;
    }

    private async Task EnsureUsuarioRolAsync(int usuarioId, int rolId, CancellationToken cancellationToken)
    {
        var exists = await _db.UsuarioRoles.AnyAsync(ur => ur.UsuarioId == usuarioId && ur.RolId == rolId, cancellationToken);
        if (exists)
            return;

        _db.UsuarioRoles.Add(new UsuarioRol { UsuarioId = usuarioId, RolId = rolId });
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<int> EnsureProductorAsync(
        int usuarioId,
        int tipoDocumentoId,
        string documento,
        string nombre,
        string? telefono,
        CancellationToken cancellationToken)
    {
        var p = await _db.Productores.FirstOrDefaultAsync(x => x.UsuarioId == usuarioId, cancellationToken);
        if (p is not null)
            return p.ProductorId;

        p = new Productor
        {
            UsuarioId = usuarioId,
            TipoDocumentoId = tipoDocumentoId,
            Documento = documento,
            Nombre = nombre,
            Telefono = telefono
        };
        _db.Productores.Add(p);
        await _db.SaveChangesAsync(cancellationToken);
        return p.ProductorId;
    }

    private async Task<int> EnsureFincaAsync(
        int productorId,
        int municipioId,
        string nombre,
        string? direccion,
        decimal latitud,
        decimal longitud,
        CancellationToken cancellationToken)
    {
        var f = await _db.Fincas.FirstOrDefaultAsync(x => x.ProductorId == productorId && x.Nombre == nombre, cancellationToken);
        if (f is not null)
        {
            f.Latitud = latitud;
            f.Longitud = longitud;
            if (!string.IsNullOrWhiteSpace(direccion))
                f.Direccion = direccion;
            await _db.SaveChangesAsync(cancellationToken);
            return f.FincaId;
        }

        f = new Finca
        {
            ProductorId = productorId,
            MunicipioId = municipioId,
            Nombre = nombre,
            Direccion = direccion,
            Latitud = latitud,
            Longitud = longitud
        };
        _db.Fincas.Add(f);
        await _db.SaveChangesAsync(cancellationToken);
        return f.FincaId;
    }

    private async Task EnsureOrdenosRecientesAsync(int fincaId, int days, CancellationToken cancellationToken)
    {
        var since = DateTime.UtcNow.Date.AddDays(-days);
        var any = await _db.Ordenos.AnyAsync(o => o.FincaId == fincaId && o.FechaHoraInicio >= since, cancellationToken);
        if (any)
            return;

        var rng = new Random(12345);
        for (var i = days; i >= 1; i--)
        {
            var start = DateTime.UtcNow.Date.AddDays(-i).AddHours(6);
            var liters = 18m + (decimal)rng.NextDouble() * 6m; // 18..24
            if (i <= 3) liters -= 2m; // caída reciente para probar

            _db.Ordenos.Add(new Ordeno
            {
                FincaId = fincaId,
                FechaHoraInicio = start,
                FechaHoraFin = start.AddHours(2),
                VolumenLitros = Math.Round(liters, 2)
            });
        }
    }

    private async Task<int> EnsureTransporteAsync(string placaVehiculo, CancellationToken cancellationToken)
    {
        var t = await _db.Transportes.FirstOrDefaultAsync(x => x.PlacaVehiculo == placaVehiculo, cancellationToken);
        if (t is not null)
            return t.TransporteId;

        t = new Transporte
        {
            PlacaVehiculo = placaVehiculo,
            FechaHoraSalida = DateTime.UtcNow.AddHours(-2),
            TemperaturaInicio = 4
        };
        _db.Transportes.Add(t);
        await _db.SaveChangesAsync(cancellationToken);
        return t.TransporteId;
    }

    private async Task<int> GetUltimoOrdenoIdAsync(int fincaId, CancellationToken cancellationToken)
    {
        var ordenoId = await _db.Ordenos
            .Where(o => o.FincaId == fincaId)
            .OrderByDescending(o => o.FechaHoraInicio)
            .Select(o => o.OrdenoId)
            .FirstOrDefaultAsync(cancellationToken);

        if (ordenoId == 0)
            throw new InvalidOperationException("No se pudo determinar un ordeño para crear el lote.");

        return ordenoId;
    }

    private async Task<int> EnsureLoteAsync(int ordenoId, int centroAcopioId, int? transporteId, CancellationToken cancellationToken)
    {
        var lote = await _db.Lotes.FirstOrDefaultAsync(l => l.OrdenoId == ordenoId && l.CentroAcopioId == centroAcopioId, cancellationToken);
        if (lote is not null)
            return lote.LoteId;

        lote = new Lote
        {
            OrdenoId = ordenoId,
            CentroAcopioId = centroAcopioId,
            VolumenCapturadoLitros = 20m,
            TransporteId = transporteId
        };
        _db.Lotes.Add(lote);
        await _db.SaveChangesAsync(cancellationToken);
        return lote.LoteId;
    }

    private async Task<int> EnsureParametroCalidadAsync(
        string nombre,
        string? unidad,
        decimal? valorMin,
        decimal? valorMax,
        CancellationToken cancellationToken)
    {
        var p = await _db.ParametrosCalidad.FirstOrDefaultAsync(x => x.Nombre == nombre, cancellationToken);
        if (p is not null)
            return p.ParametroId;

        p = new ParametroCalidad
        {
            Nombre = nombre,
            Unidad = unidad,
            ValorMinimo = valorMin,
            ValorMaximo = valorMax
        };
        _db.ParametrosCalidad.Add(p);
        await _db.SaveChangesAsync(cancellationToken);
        return p.ParametroId;
    }

    private async Task EnsureAnalisisAcidezAsync(
        int fincaId,
        int centroAcopioId,
        int tecnicoUsuarioId,
        int parametroAcidezId,
        CancellationToken cancellationToken)
    {
        // Si ya hay resultados de acidez para esta finca, no duplicar
        var exists = await _db.ResultadosParametro.AnyAsync(r =>
            r.ParametroId == parametroAcidezId &&
            r.Analisis.Muestra.Lote.Ordeno.FincaId == fincaId,
            cancellationToken);

        if (exists)
            return;

        // Crear lote y muestra ligada a un ordeño reciente para trazar hasta la finca
        var ordenoId = await GetUltimoOrdenoIdAsync(fincaId, cancellationToken);
        var loteId = await EnsureLoteAsync(ordenoId, centroAcopioId, transporteId: null, cancellationToken);

        var muestra = new Muestra
        {
            LoteId = loteId,
            TecnicoPorUsuarioId = tecnicoUsuarioId,
            FechaHoraToma = DateTime.UtcNow.AddDays(-2)
        };
        _db.Muestras.Add(muestra);
        await _db.SaveChangesAsync(cancellationToken);

        // Dos análisis con pH decreciente para activar heurística/alerta
        var a1 = new AnalisisCalidad
        {
            MuestraId = muestra.MuestraId,
            FechaHoraAnalisis = DateTime.UtcNow.AddDays(-2).AddHours(2),
            Observaciones = "Análisis base"
        };
        var a2 = new AnalisisCalidad
        {
            MuestraId = muestra.MuestraId,
            FechaHoraAnalisis = DateTime.UtcNow.AddDays(-1).AddHours(2),
            Observaciones = "Análisis reciente"
        };
        _db.AnalisisCalidad.AddRange(a1, a2);
        await _db.SaveChangesAsync(cancellationToken);

        _db.ResultadosParametro.AddRange(
            new ResultadoParametro
            {
                AnalisisId = a1.AnalisisId,
                ParametroId = parametroAcidezId,
                ValorResultado = 6.70m,
                Observacion = "Dentro de rango"
            },
            new ResultadoParametro
            {
                AnalisisId = a2.AnalisisId,
                ParametroId = parametroAcidezId,
                ValorResultado = 6.45m,
                Observacion = "Tendencia a la baja"
            });
    }
}

