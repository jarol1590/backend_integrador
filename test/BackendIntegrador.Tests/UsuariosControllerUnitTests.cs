using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using BackendIntegrador.Api.Controllers;
using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace BackendIntegrador.Tests;

public class UsuariosControllerUnitTests
{
    private readonly Mock<IUsuarioFacadeService> _mockFacade;
    private readonly Mock<IUserManagementService> _mockUserManagement;
    private readonly UsuariosController _controller;

    public UsuariosControllerUnitTests()
    {
        _mockFacade = new Mock<IUsuarioFacadeService>();
        _mockUserManagement = new Mock<IUserManagementService>();
        _controller = new UsuariosController(_mockFacade.Object, _mockUserManagement.Object);
    }

    [Fact]
    public async Task Create_ValidUsuario_ReturnsCreatedAndContainsId()
    {
        var dto = new ProvisionarUsuarioDto(
            "user@example.com",
            "Secret123!",
            "activo",
            1,
            new List<int> { 1 },
            null);

        var createdUser = BuildPerfil(1, "user@example.com");

        _mockFacade.Setup(s => s.ProvisionarAsync(It.IsAny<ProvisionarUsuarioDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUser);

        var result = await _controller.Create(dto, CancellationToken.None);

        var createdResult = result.Result as CreatedAtActionResult;
        createdResult.Should().NotBeNull();
        createdResult!.StatusCode.Should().Be(201);
        createdResult.RouteValues.Should().ContainKey("id");
        createdResult.RouteValues["id"].Should().Be(1);
    }

    [Fact]
    public async Task Create_InvalidEmail_ReturnsBadRequest()
    {
        var dto = new ProvisionarUsuarioDto("usuario@mal", "12345", "activo", 1, new List<int> { 1 }, null);

        _mockFacade.Setup(s => s.ProvisionarAsync(It.IsAny<ProvisionarUsuarioDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("El email tiene un formato inválido"));

        var result = await _controller.Create(dto, CancellationToken.None);

        var badRequest = result.Result as BadRequestObjectResult;
        badRequest.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithList()
    {
        var list = new List<UsuarioListadoDto>
        {
            new(1, "user@example.com", "activo", DateTime.Parse("2026-04-29T00:00:00Z"), "Centro", new[] { "Admin" }, "admin")
        };
        _mockFacade.Setup(s => s.GetListadoAsync(It.IsAny<CancellationToken>())).ReturnsAsync(list);

        var action = await _controller.GetAll(CancellationToken.None);

        var ok = action.Result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.Value.Should().BeEquivalentTo(list);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsOkWithItem()
    {
        var user = BuildPerfil(1, "user@example.com");
        _mockFacade.Setup(s => s.GetPerfilAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var action = await _controller.GetById(1, CancellationToken.None);

        var ok = action.Result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.Value.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task GetMe_ReturnsOkWithCurrentUser()
    {
        var user = BuildPerfil(5, "me@example.com");
        _mockFacade.Setup(s => s.GetPerfilAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "5") }, "TestAuth");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        var action = await _controller.GetMe(CancellationToken.None);

        var ok = action.Result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.Value.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task Update_Valid_ReturnsOk()
    {
        var updateDto = new ActualizarUsuarioDto("user@example.com", "activo", 1, "NewSecret123!", new List<int> { 1 }, null);
        _mockFacade.Setup(s => s.ActualizarAsync(1, It.IsAny<ActualizarUsuarioDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildPerfil(1, "user@example.com"));

        var action = await _controller.Update(1, updateDto, CancellationToken.None);

        var ok = action as OkObjectResult;
        ok.Should().NotBeNull();
    }

    [Fact]
    public async Task Delete_ExistingId_ReturnsOk()
    {
        _mockFacade.Setup(s => s.DesactivarAsync(2, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var action = await _controller.Delete(2, CancellationToken.None);

        action.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_NotFoundId_ReturnsNotFound()
    {
        _mockFacade.Setup(s => s.DesactivarAsync(50, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException());

        var action = await _controller.Delete(50, CancellationToken.None);

        action.Should().BeOfType<NotFoundObjectResult>();
    }

    private static UsuarioPerfilDto BuildPerfil(int id, string email) =>
        new(
            id,
            email,
            "activo",
            DateTime.Parse("2026-04-29T00:00:00Z"),
            null,
            new List<RolResumenDto>(),
            new UsuarioAlcanceDto("sin_asignar", null, Array.Empty<FincaAlcanceDto>()));
}
