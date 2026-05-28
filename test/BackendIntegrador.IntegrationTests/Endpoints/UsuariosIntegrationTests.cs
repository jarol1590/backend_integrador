using BackendIntegrador.Application.Common;
using BackendIntegrador.Application.Dtos;
using BackendIntegrador.IntegrationTests.Common;
using FluentAssertions;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace BackendIntegrador.IntegrationTests.Endpoints;

public class UsuariosIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateUsuario_WithValidData_ReturnsCreatedStatusAndUser()
    {
        var createDto = new CreateUsuarioDto(
            Email: "newuser@example.com",
            Password: "SecurePassword123!",
            Estado: "activo",
            CentroAcopioId: null);

        var content = new StringContent(
            JsonSerializer.Serialize(createDto),
            Encoding.UTF8,
            "application/json");

        var response = await HttpClient.PostAsync("/api/usuarios", content);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetAllUsuarios_WithoutToken_ReturnsUnauthorized()
    {
        var response = await HttpClient.GetAsync("/api/usuarios");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}