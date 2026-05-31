using BackendIntegrador.Application.Common;
using BackendIntegrador.Application.Dtos;
using BackendIntegrador.IntegrationTests.Common;
using FluentAssertions;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Xunit;

namespace BackendIntegrador.IntegrationTests.Endpoints;

public class UsuariosIntegrationTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public async Task CreateUsuario_WithValidData_ReturnsCreatedStatusAndUser()
    {
        var roleId = await SeedRolAsync(UsuarioRoleTypes.RolNombreAdministrador);

        var createDto = new ProvisionarUsuarioDto(
            Email: "newuser@example.com",
            Password: "SecurePassword123!",
            Estado: "activo",
            RolId: roleId,
            CentroAcopioId: null,
            Productor: null);

        var content = new StringContent(
            JsonSerializer.Serialize(createDto, JsonOptions),
            Encoding.UTF8,
            "application/json");

        var response = await HttpClient.PostAsync("/api/usuarios", content);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetAllUsuarios_WithoutToken_ReturnsUnauthorized()
    {
        using var client = Factory.CreateClient();
        var response = await client.GetAsync("/api/usuarios");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
