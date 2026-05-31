using BackendIntegrador.Api;
using BackendIntegrador.Application.Common;
using BackendIntegrador.Domain.Entities;
using BackendIntegrador.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Net.Http.Headers;
using Xunit;

namespace BackendIntegrador.IntegrationTests.Common;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncDisposable
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.FirstOrDefault(d => 
                d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(_connection));
        });

        builder.UseEnvironment("Test");
    }

    public async override ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }

        await base.DisposeAsync();
    }
}

public class IntegrationTestBase : IAsyncLifetime
{
    protected readonly HttpClient HttpClient;
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly JwtSettings JwtSettings;
    protected readonly JwtTokenGenerator JwtTokenGenerator;

    public IntegrationTestBase()
    {
        Factory = new CustomWebApplicationFactory();
        HttpClient = Factory.CreateClient();
        JwtSettings = Factory.Services.GetRequiredService<JwtSettings>();
        JwtTokenGenerator = new JwtTokenGenerator(JwtSettings.SecretKey, JwtSettings.Issuer, JwtSettings.Audience);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JwtTokenGenerator.GenerateToken(1, "test@example.com"));
    }

    public async Task InitializeAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        HttpClient?.Dispose();
        await Factory.DisposeAsync();
    }

    protected async Task<int> SeedDepartamentoAsync(string nombre = "Departamento Test")
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var entity = new Departamento { Nombre = nombre };
        dbContext.Departamentos.Add(entity);
        await dbContext.SaveChangesAsync();
        return entity.DepartamentoId;
    }

    protected async Task<int> SeedTipoDocumentoAsync(string nombre = "Documento Test")
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var entity = new TipoDocumento { Nombre = nombre };
        dbContext.TiposDocumento.Add(entity);
        await dbContext.SaveChangesAsync();
        return entity.TipoDocumentoId;
    }

    protected async Task<int> SeedMunicipioAsync(int departamentoId, string nombre = "Municipio Test")
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var entity = new Municipio { Nombre = nombre, DepartamentoId = departamentoId };
        dbContext.Municipios.Add(entity);
        await dbContext.SaveChangesAsync();
        return entity.MunicipioId;
    }

    protected async Task<int> SeedCentroAcopioAsync(int municipioId, string nombre = "Centro Test")
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var entity = new CentroAcopio { Nombre = nombre, MunicipioId = municipioId };
        dbContext.CentrosAcopio.Add(entity);
        await dbContext.SaveChangesAsync();
        return entity.CentroAcopioId;
    }

    protected async Task<int> SeedUsuarioAsync(string email = "seeduser@example.com", int? centroAcopioId = null)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var entity = new Usuario
        {
            Email = email,
            PasswordHash = "hash",
            Estado = "activo",
            FechaCreacion = DateTime.UtcNow,
            CentroAcopioId = centroAcopioId
        };
        dbContext.Usuarios.Add(entity);
        await dbContext.SaveChangesAsync();
        return entity.UsuarioId;
    }

    protected async Task<int> SeedProductorAsync(int usuarioId, int tipoDocumentoId, string documento = "1234567890")
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var entity = new Productor
        {
            Nombre = "Productor Test",
            Documento = documento,
            UsuarioId = usuarioId,
            TipoDocumentoId = tipoDocumentoId
        };
        dbContext.Productores.Add(entity);
        await dbContext.SaveChangesAsync();
        return entity.ProductorId;
    }

    protected async Task<int> SeedFincaAsync(int productorId, int municipioId)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var entity = new Finca
        {
            Nombre = "Finca Test",
            ProductorId = productorId,
            MunicipioId = municipioId
        };
        dbContext.Fincas.Add(entity);
        await dbContext.SaveChangesAsync();
        return entity.FincaId;
    }

    protected async Task<int> SeedOrdenoAsync(int fincaId)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var entity = new Ordeno
        {
            FechaHoraInicio = DateTime.UtcNow,
            VolumenLitros = 10m,
            FincaId = fincaId
        };
        dbContext.Ordenos.Add(entity);
        await dbContext.SaveChangesAsync();
        return entity.OrdenoId;
    }

    protected async Task<int> SeedTransporteAsync(string placa = "ABC123")
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var entity = new Transporte
        {
            PlacaVehiculo = placa,
            FechaHoraSalida = DateTime.UtcNow
        };
        dbContext.Transportes.Add(entity);
        await dbContext.SaveChangesAsync();
        return entity.TransporteId;
    }

    protected async Task<int> SeedLoteAsync(int ordenoId, int centroAcopioId, int? transporteId = null)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var entity = new Lote
        {
            OrdenoId = ordenoId,
            CentroAcopioId = centroAcopioId,
            VolumenCapturadoLitros = 5m,
            TransporteId = transporteId
        };
        dbContext.Lotes.Add(entity);
        await dbContext.SaveChangesAsync();
        return entity.LoteId;
    }

    protected async Task<int> SeedMuestraAsync(int loteId, int usuarioId)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var entity = new Muestra
        {
            LoteId = loteId,
            TecnicoPorUsuarioId = usuarioId,
            FechaHoraToma = DateTime.UtcNow
        };
        dbContext.Muestras.Add(entity);
        await dbContext.SaveChangesAsync();
        return entity.MuestraId;
    }

    protected async Task<int> SeedAnalisisAsync(int muestraId)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var entity = new AnalisisCalidad
        {
            MuestraId = muestraId
        };
        dbContext.AnalisisCalidad.Add(entity);
        await dbContext.SaveChangesAsync();
        return entity.AnalisisId;
    }

    protected async Task<int> SeedRolAsync(string nombre = "Rol Test")
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var entity = new Rol { Nombre = nombre, Descripcion = "Rol semilla" };
        dbContext.Roles.Add(entity);
        await dbContext.SaveChangesAsync();
        return entity.RolId;
    }

    protected async Task<int> SeedParametroAsync(string nombre = "Parametro Test")
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var entity = new ParametroCalidad
        {
            Nombre = nombre
        };
        dbContext.ParametrosCalidad.Add(entity);
        await dbContext.SaveChangesAsync();
        return entity.ParametroId;
    }
}