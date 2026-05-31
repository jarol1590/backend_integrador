using BackendIntegrador.Application.Common;
using BackendIntegrador.Application.Dtos;
using BackendIntegrador.IntegrationTests.Common;
using FluentAssertions;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace BackendIntegrador.IntegrationTests.Endpoints;

public class UsuariosPorRolIntegrationTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public async Task ProvisionarAdministrador_ReturnsPerfilSinCentroNiFincas()
    {
        var rolId = await SeedRolAsync(UsuarioRoleTypes.RolNombreAdministrador);
        var response = await PostUsuarioAsync(new ProvisionarUsuarioDto(
            "admin@test.com", "SecurePassword123!", "activo", rolId, null, null));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().NotContain("centroAcopio");
        json.Should().NotContain("fincas");
        json.Should().Contain("\"tipoUsuario\":\"administrador\"");
    }

    [Fact]
    public async Task ProvisionarCentroAcopio_ReturnsPerfilConCentroSinFincas()
    {
        var departamentoId = await SeedDepartamentoAsync("Depto Centro Rol");
        var municipioId = await SeedMunicipioAsync(departamentoId, "Mun Centro Rol");
        var centroId = await SeedCentroAcopioAsync(municipioId, "Centro Rol Test");
        var rolId = await SeedRolAsync(UsuarioRoleTypes.RolNombreCentroAcopio);

        var response = await PostUsuarioAsync(new ProvisionarUsuarioDto(
            "centro@example.com", "SecurePassword123!", "activo", rolId, centroId, null));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("\"tipoUsuario\":\"centro_acopio\"");
        json.Should().Contain("centroAcopio");
        json.Should().NotContain("fincas");
    }

    [Fact]
    public async Task ProvisionarTrabajador_ReturnsPerfilConCentroSinFincas()
    {
        var departamentoId = await SeedDepartamentoAsync();
        var municipioId = await SeedMunicipioAsync(departamentoId);
        var centroId = await SeedCentroAcopioAsync(municipioId);
        var rolId = await SeedRolAsync(UsuarioRoleTypes.RolNombreTrabajadorCentroAcopio);

        var response = await PostUsuarioAsync(new ProvisionarUsuarioDto(
            "trabajador@test.com", "SecurePassword123!", "activo", rolId, centroId, null));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("\"tipoUsuario\":\"trabajador_centro_acopio\"");
        json.Should().Contain("centroAcopio");
        json.Should().NotContain("fincas");
        json.Should().NotContain("productor");
    }

    [Fact]
    public async Task ProvisionarProductor_ReturnsPerfilConProductor()
    {
        var rolId = await SeedRolAsync(UsuarioRoleTypes.RolNombreProductor);
        var tipoDocId = await SeedTipoDocumentoAsync("CC Productor Rol");

        var response = await PostUsuarioAsync(new ProvisionarUsuarioDto(
            "productor@test.com",
            "SecurePassword123!",
            "activo",
            rolId,
            null,
            new ProductorProvisionDto("Ana López", "99887766", "3001112233", tipoDocId, null)));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("\"tipoUsuario\":\"productor\"");
        json.Should().Contain("productor");
        json.Should().NotContain("centroAcopio");
    }

    [Fact]
    public async Task ProvisionarProductorConCentroAcopio_ReturnsBadRequest()
    {
        var departamentoId = await SeedDepartamentoAsync();
        var municipioId = await SeedMunicipioAsync(departamentoId);
        var centroId = await SeedCentroAcopioAsync(municipioId);
        var rolId = await SeedRolAsync(UsuarioRoleTypes.RolNombreProductor);
        var tipoDocId = await SeedTipoDocumentoAsync("CC Invalid Productor");

        var response = await PostUsuarioAsync(new ProvisionarUsuarioDto(
            "bad-productor@test.com",
            "SecurePassword123!",
            "activo",
            rolId,
            centroId,
            new ProductorProvisionDto("Bad", "11223344", null, tipoDocId, null)));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ProvisionarTrabajadorConProductor_ReturnsBadRequest()
    {
        var departamentoId = await SeedDepartamentoAsync("Depto Trab Bad");
        var municipioId = await SeedMunicipioAsync(departamentoId, "Mun Trab Bad");
        var centroId = await SeedCentroAcopioAsync(municipioId, "Centro Trab Bad");
        var rolId = await SeedRolAsync(UsuarioRoleTypes.RolNombreTrabajadorCentroAcopio);
        var tipoDocId = await SeedTipoDocumentoAsync("CC Trab Bad");

        var response = await PostUsuarioAsync(new ProvisionarUsuarioDto(
            "bad-trab@test.com",
            "SecurePassword123!",
            "activo",
            rolId,
            centroId,
            new ProductorProvisionDto("X", "55667788", null, tipoDocId, null)));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task<HttpResponseMessage> PostUsuarioAsync(ProvisionarUsuarioDto dto)
    {
        var content = new StringContent(JsonSerializer.Serialize(dto, JsonOptions), Encoding.UTF8, "application/json");
        return await HttpClient.PostAsync("/api/usuarios", content);
    }
}
