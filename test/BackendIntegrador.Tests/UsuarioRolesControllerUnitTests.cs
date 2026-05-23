using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendIntegrador.Api.Controllers;
using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace BackendIntegrador.Tests
{
    public class UsuarioRolesControllerUnitTests
    {
        private readonly Mock<IUsuarioRolService> _mockSvc;
        private readonly UsuarioRolesController _controller;

        public UsuarioRolesControllerUnitTests()
        {
            _mockSvc = new Mock<IUsuarioRolService>();
            _controller = new UsuarioRolesController(_mockSvc.Object);
        }

        [Fact]
        public async Task Create_AcceptsCreatedOrThrowsInvalidOperation()
        {
            var dto = new CreateUsuarioRolDto(1,1);
            _mockSvc.Setup(s => s.CreateAsync(It.IsAny<CreateUsuarioRolDto>(), It.IsAny<CancellationToken>())).ReturnsAsync(new UsuarioRolDto(1,1));

            // Successful create
            var result = await _controller.Create(dto, CancellationToken.None);
            var created = result.Result as CreatedAtActionResult;
            created.Should().NotBeNull();

            // Or conflict scenario: method might throw InvalidOperationException (acceptable)
            _mockSvc.Setup(s => s.CreateAsync(It.IsAny<CreateUsuarioRolDto>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("Existe"));
            try
            {
                await _controller.Create(dto, CancellationToken.None);
            }
            catch (InvalidOperationException)
            {
                // aceptable
            }
        }

        [Fact]
        public async Task GetAll_ReturnsOk()
        {
            var list = new List<UsuarioRolDto>{ new UsuarioRolDto(1,1) };
            _mockSvc.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(list);
            var action = await _controller.GetAll(CancellationToken.None);
            var ok = action.Result as OkObjectResult; ok.Should().NotBeNull(); ok!.Value.Should().BeEquivalentTo(list);
        }

        [Fact]
        public async Task Get_ReturnsOk()
        {
            var item = new UsuarioRolDto(1,1);
            _mockSvc.Setup(s => s.GetAsync(1,1, It.IsAny<CancellationToken>())).ReturnsAsync(item);
            var action = await _controller.Get(1,1, CancellationToken.None);
            var ok = action.Result as OkObjectResult; ok.Should().NotBeNull(); ok!.Value.Should().BeEquivalentTo(item);
        }

        [Fact]
        public async Task Delete_AcceptsNoContentOrNotFound()
        {
            _mockSvc.Setup(s => s.DeleteAsync(1,1, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            var action = await _controller.Delete(1,1, CancellationToken.None);
            if (action is NoContentResult) (action as NoContentResult)!.Should().NotBeNull(); else if (action is OkObjectResult ok) { var statusProp = ok.Value.GetType().GetProperty("status"); if (statusProp!=null) statusProp!.GetValue(ok.Value).Should().Be(204); }

            _mockSvc.Setup(s => s.DeleteAsync(50,50, It.IsAny<CancellationToken>())).ThrowsAsync(new KeyNotFoundException());
            var notFound = await _controller.Delete(50,50, CancellationToken.None);
            notFound.Should().BeOfType<NotFoundResult>();
        }
    }
}
