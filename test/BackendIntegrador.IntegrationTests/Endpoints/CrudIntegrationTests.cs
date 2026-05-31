using BackendIntegrador.Application.Dtos;
using BackendIntegrador.IntegrationTests.Common;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BackendIntegrador.IntegrationTests.Endpoints;

public class CrudIntegrationTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    [Theory]
    [InlineData("api/departamentos")]
    [InlineData("api/municipios")]
    [InlineData("api/centros-acopio")]
    [InlineData("api/tipos-documento")]
    [InlineData("api/roles")]
    [InlineData("api/parametros-calidad")]
    [InlineData("api/transportes")]
    [InlineData("api/ordenos")]
    [InlineData("api/lotes")]
    [InlineData("api/recepciones-acopio")]
    [InlineData("api/muestras")]
    [InlineData("api/analisis-calidad")]
    [InlineData("api/resultados-parametro")]
    [InlineData("api/productores")]
    [InlineData("api/fincas")]
    [InlineData("api/usuarios")]
    public async Task GetAllEndpoints_ReturnsOk(string route)
    {
        var response = await HttpClient.GetAsync(route);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateDepartamento_ReturnsCreated()
    {
        var dto = new CreateDepartamentoDto("Departamento Test");
        var response = await PostJsonAsync("api/departamentos", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateTipoDocumento_ReturnsCreated()
    {
        var dto = new CreateTipoDocumentoDto("Documento Test", "Descripción opcional");
        var response = await PostJsonAsync("api/tipos-documento", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateParametroCalidad_ReturnsCreated()
    {
        var dto = new CreateParametroCalidadDto("pH", "unidad", 0.1m, 14.0m);
        var response = await PostJsonAsync("api/parametros-calidad", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateRol_ReturnsCreated()
    {
        var dto = new CreateRolDto("Admin", "Rol de administración");
        var response = await PostJsonAsync("api/roles", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateMunicipio_ReturnsCreated()
    {
        var departamentoId = await SeedDepartamentoAsync();
        var dto = new CreateMunicipioDto("Municipio Test", departamentoId);
        var response = await PostJsonAsync("api/municipios", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateCentroAcopio_ReturnsCreated()
    {
        var departamentoId = await SeedDepartamentoAsync();
        var municipioId = await SeedMunicipioAsync(departamentoId);
        var dto = new CreateCentroAcopioDto("Centro Test", "Dirección Test", 4.5m, -75.6m, municipioId);
        var response = await PostJsonAsync("api/centros-acopio", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateTransporte_ReturnsCreated()
    {
        var dto = new CreateTransporteDto("ABC123", DateTime.UtcNow, null, 25);
        var response = await PostJsonAsync("api/transportes", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateProductor_ReturnsCreated()
    {
        var tipoDocumentoId = await SeedTipoDocumentoAsync();
        var usuarioId = await SeedUsuarioAsync("productor@example.com");
        var dto = new CreateProductorDto("Productor Test", "1234567890", "3201234567", usuarioId, tipoDocumentoId);
        var response = await PostJsonAsync("api/productores", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateFinca_ReturnsCreated()
    {
        var departamentoId = await SeedDepartamentoAsync();
        var municipioId = await SeedMunicipioAsync(departamentoId);
        var usuarioId = await SeedUsuarioAsync("productor2@example.com");
        var tipoDocumentoId = await SeedTipoDocumentoAsync("Documento Finca");
        var productorId = await SeedProductorAsync(usuarioId, tipoDocumentoId, "0987654321");
        var dto = new CreateFincaDto("Finca Test", "Dirección Finca", 4.6m, -75.7m, productorId, municipioId);
        var response = await PostJsonAsync("api/fincas", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateOrdeno_ReturnsCreated()
    {
        var departamentoId = await SeedDepartamentoAsync();
        var municipioId = await SeedMunicipioAsync(departamentoId);
        var usuarioId = await SeedUsuarioAsync("productor3@example.com");
        var tipoDocumentoId = await SeedTipoDocumentoAsync("Documento Ordeno");
        var productorId = await SeedProductorAsync(usuarioId, tipoDocumentoId, "1122334455");
        var fincaId = await SeedFincaAsync(productorId, municipioId);
        var dto = new CreateOrdenoDto(DateTime.UtcNow, null, 12.34m, fincaId);
        var response = await PostJsonAsync("api/ordenos", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateLote_ReturnsCreated()
    {
        var departamentoId = await SeedDepartamentoAsync();
        var municipioId = await SeedMunicipioAsync(departamentoId);
        var centroId = await SeedCentroAcopioAsync(municipioId);
        var usuarioId = await SeedUsuarioAsync("loteuser@example.com", centroId);
        var tipoDocumentoId = await SeedTipoDocumentoAsync("Documento Lote");
        var productorId = await SeedProductorAsync(usuarioId, tipoDocumentoId, "5544332211");
        var fincaId = await SeedFincaAsync(productorId, municipioId);
        var ordenoId = await SeedOrdenoAsync(fincaId);
        var transporteId = await SeedTransporteAsync("LOT123");

        var dto = new CreateLoteDto(ordenoId, centroId, 8.75m, transporteId);
        var response = await PostJsonAsync("api/lotes", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateRecepcionAcopio_ReturnsCreated()
    {
        var departamentoId = await SeedDepartamentoAsync();
        var municipioId = await SeedMunicipioAsync(departamentoId);
        var centroId = await SeedCentroAcopioAsync(municipioId);
        var usuarioId = await SeedUsuarioAsync("reception@example.com", centroId);
        var transporteId = await SeedTransporteAsync("REC123");

        var dto = new CreateRecepcionAcopioDto(transporteId, centroId, DateTime.UtcNow, usuarioId, 20, 123.45m);
        var response = await PostJsonAsync("api/recepciones-acopio", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateMuestra_ReturnsCreated()
    {
        var departamentoId = await SeedDepartamentoAsync();
        var municipioId = await SeedMunicipioAsync(departamentoId);
        var centroId = await SeedCentroAcopioAsync(municipioId);
        var usuarioId = await SeedUsuarioAsync("muestrauser@example.com", centroId);
        var tipoDocumentoId = await SeedTipoDocumentoAsync("Documento Muestra");
        var productorId = await SeedProductorAsync(usuarioId, tipoDocumentoId, "6677889900");
        var fincaId = await SeedFincaAsync(productorId, municipioId);
        var ordenoId = await SeedOrdenoAsync(fincaId);
        var loteId = await SeedLoteAsync(ordenoId, centroId);

        var dto = new CreateMuestraDto(loteId, usuarioId, DateTime.UtcNow);
        var response = await PostJsonAsync("api/muestras", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateAnalisisCalidad_ReturnsCreated()
    {
        var departamentoId = await SeedDepartamentoAsync();
        var municipioId = await SeedMunicipioAsync(departamentoId);
        var centroId = await SeedCentroAcopioAsync(municipioId);
        var usuarioId = await SeedUsuarioAsync("analisisuser@example.com", centroId);
        var tipoDocumentoId = await SeedTipoDocumentoAsync("Documento Analisis");
        var productorId = await SeedProductorAsync(usuarioId, tipoDocumentoId, "2233445566");
        var fincaId = await SeedFincaAsync(productorId, municipioId);
        var ordenoId = await SeedOrdenoAsync(fincaId);
        var loteId = await SeedLoteAsync(ordenoId, centroId);
        var muestraId = await SeedMuestraAsync(loteId, usuarioId);

        var dto = new CreateAnalisisCalidadDto(muestraId, DateTime.UtcNow, "Observación de prueba");
        var response = await PostJsonAsync("api/analisis-calidad", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateResultadoParametro_ReturnsCreated()
    {
        var departamentoId = await SeedDepartamentoAsync();
        var municipioId = await SeedMunicipioAsync(departamentoId);
        var centroId = await SeedCentroAcopioAsync(municipioId);
        var usuarioId = await SeedUsuarioAsync("resultado@example.com", centroId);
        var tipoDocumentoId = await SeedTipoDocumentoAsync("Documento Resultado");
        var productorId = await SeedProductorAsync(usuarioId, tipoDocumentoId, "3344556677");
        var fincaId = await SeedFincaAsync(productorId, municipioId);
        var ordenoId = await SeedOrdenoAsync(fincaId);
        var loteId = await SeedLoteAsync(ordenoId, centroId);
        var muestraId = await SeedMuestraAsync(loteId, usuarioId);
        var analisisId = await SeedAnalisisAsync(muestraId);
        var parametroId = await SeedParametroAsync("Resultado Param Test");

        var dto = new CreateResultadoParametroDto(analisisId, parametroId, 7.25m, "OK");
        var response = await PostJsonAsync("api/resultados-parametro", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task ProvisionarUsuario_ReturnsCreated()
    {
        var roleId = await SeedRolAsync("Rol Provision Test");

        var dto = new ProvisionarUsuarioDto(
            "roluser@example.com",
            "SecurePassword123!",
            "activo",
            null,
            new List<int> { roleId },
            null);

        var response = await PostJsonAsync("api/usuarios", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    private async Task<HttpResponseMessage> PostJsonAsync<T>(string url, T dto)
    {
        var content = new StringContent(JsonSerializer.Serialize(dto, JsonOptions), Encoding.UTF8, "application/json");
        return await HttpClient.PostAsync(url, content);
    }
}
