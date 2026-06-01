using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Common;
using BackendIntegrador.Domain.Entities;
using BackendIntegrador.Infrastructure.Persistence;
using BackendIntegrador.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Xunit;

namespace BackendIntegrador.IntegrationTests.Endpoints;

public class GemeloDigitalIntegrationTests
{
    [Fact]
    public async Task SincronizarGemelo_WithCoordinates_PersistsEstadoAndLecturas()
    {
        await using var factory = new GemeloWebApplicationFactory();
        using var client = factory.CreateClient();
        var jwtSettings = factory.Services.GetRequiredService<JwtSettings>();
        var jwt = new JwtTokenGenerator(jwtSettings.SecretKey, jwtSettings.Issuer, jwtSettings.Audience);

        var (fincaId, usuarioId) = await SeedFincaWithCoordsAsync(factory);

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt.GenerateToken(usuarioId, "admin-gemelo@test.com"));

        var syncResponse = await client.PostAsync($"/api/fincas/{fincaId}/gemelo/sincronizar", null);
        syncResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var estadoResponse = await client.GetAsync($"/api/fincas/{fincaId}/gemelo/estado");
        estadoResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.LecturasClimaticas.Count(l => l.FincaId == fincaId).Should().BeGreaterThan(0);

        var estado = db.FincasGemeloEstado.First(e => e.FincaId == fincaId);
        estado.EstadoSync.Should().Be(GemeloSyncEstados.Ok);
    }

    [Fact]
    public async Task SincronizarGemelo_WithoutCoordinates_ReturnsBadRequest()
    {
        await using var factory = new GemeloWebApplicationFactory();
        using var client = factory.CreateClient();
        var jwtSettings = factory.Services.GetRequiredService<JwtSettings>();
        var jwt = new JwtTokenGenerator(jwtSettings.SecretKey, jwtSettings.Issuer, jwtSettings.Audience);

        var (fincaId, usuarioId) = await SeedFincaWithoutCoordsAsync(factory);

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt.GenerateToken(usuarioId, "user-nocoords@test.com"));

        var response = await client.PostAsync($"/api/fincas/{fincaId}/gemelo/sincronizar", null);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static async Task<(int FincaId, int UsuarioId)> SeedFincaWithCoordsAsync(GemeloWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();

        var dep = new Departamento { Nombre = "Dep Gemelo" };
        db.Departamentos.Add(dep);
        await db.SaveChangesAsync();

        var mun = new Municipio { Nombre = "Mun Gemelo", DepartamentoId = dep.DepartamentoId };
        db.Municipios.Add(mun);
        await db.SaveChangesAsync();

        var rol = new Rol { Nombre = UsuarioRoleTypes.RolNombreAdministrador, Descripcion = "Admin" };
        db.Roles.Add(rol);
        await db.SaveChangesAsync();

        var usuario = new Usuario
        {
            Email = "admin-gemelo@test.com",
            PasswordHash = "hash",
            Estado = "activo",
            FechaCreacion = DateTime.UtcNow
        };
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();

        db.UsuarioRoles.Add(new UsuarioRol { UsuarioId = usuario.UsuarioId, RolId = rol.RolId });
        await db.SaveChangesAsync();

        var tipoDoc = new TipoDocumento { Nombre = "CC Gemelo" };
        db.TiposDocumento.Add(tipoDoc);
        await db.SaveChangesAsync();

        var productor = new Productor
        {
            Nombre = "Productor Gemelo",
            Documento = "GEM001",
            UsuarioId = usuario.UsuarioId,
            TipoDocumentoId = tipoDoc.TipoDocumentoId
        };
        db.Productores.Add(productor);
        await db.SaveChangesAsync();

        var finca = new Finca
        {
            Nombre = "Finca Gemelo",
            ProductorId = productor.ProductorId,
            MunicipioId = mun.MunicipioId,
            Latitud = 5.0689m,
            Longitud = -75.5174m
        };
        db.Fincas.Add(finca);
        await db.SaveChangesAsync();

        return (finca.FincaId, usuario.UsuarioId);
    }

    private static async Task<(int FincaId, int UsuarioId)> SeedFincaWithoutCoordsAsync(GemeloWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();

        var dep = new Departamento { Nombre = "Dep Sin Coords" };
        db.Departamentos.Add(dep);
        await db.SaveChangesAsync();

        var mun = new Municipio { Nombre = "Mun Sin Coords", DepartamentoId = dep.DepartamentoId };
        db.Municipios.Add(mun);
        await db.SaveChangesAsync();

        var rol = new Rol { Nombre = UsuarioRoleTypes.RolNombreAdministrador, Descripcion = "Admin" };
        db.Roles.Add(rol);
        await db.SaveChangesAsync();

        var usuario = new Usuario
        {
            Email = "user-nocoords@test.com",
            PasswordHash = "hash",
            Estado = "activo",
            FechaCreacion = DateTime.UtcNow
        };
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();

        db.UsuarioRoles.Add(new UsuarioRol { UsuarioId = usuario.UsuarioId, RolId = rol.RolId });
        await db.SaveChangesAsync();

        var tipoDoc = new TipoDocumento { Nombre = "CC Sin Coords" };
        db.TiposDocumento.Add(tipoDoc);
        await db.SaveChangesAsync();

        var productor = new Productor
        {
            Nombre = "Productor Sin Coords",
            Documento = "NOCOORD1",
            UsuarioId = usuario.UsuarioId,
            TipoDocumentoId = tipoDoc.TipoDocumentoId
        };
        db.Productores.Add(productor);
        await db.SaveChangesAsync();

        var finca = new Finca
        {
            Nombre = "Finca Sin Coords",
            ProductorId = productor.ProductorId,
            MunicipioId = mun.MunicipioId
        };
        db.Fincas.Add(finca);
        await db.SaveChangesAsync();

        return (finca.FincaId, usuario.UsuarioId);
    }
}

internal sealed class GemeloWebApplicationFactory : WebApplicationFactory<Program>, IAsyncDisposable
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));

            var climateDescriptors = services
                .Where(d => d.ServiceType == typeof(IClimateDataProvider))
                .ToList();
            foreach (var d in climateDescriptors)
                services.Remove(d);

            services.AddSingleton<IClimateDataProvider, FakeClimateDataProvider>();
        });

        builder.UseEnvironment("Test");
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }

        Dispose();
    }
}
