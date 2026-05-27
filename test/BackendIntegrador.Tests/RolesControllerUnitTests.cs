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
    public class RolesControllerUnitTests
    {
        private readonly Mock<ICrudService<RolDto, CreateRolDto, UpdateRolDto>> _mockSvc;
        private readonly RolesController _controller;

        public RolesControllerUnitTests()
        {
            _mockSvc = new Mock<ICrudService<RolDto, CreateRolDto, UpdateRolDto>>();
            _controller = new RolesController(_mockSvc.Object);
        }

        [Fact]
        public async Task Create_RolExists_AcceptsConflictOrBadRequest()
        {
            var dto = new CreateRolDto("Admin", null);
            _mockSvc.Setup(s => s.CreateAsync(It.IsAny<CreateRolDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Rol ya existe"));

            var result = await _controller.Create(dto, CancellationToken.None);

            if (result.Result is BadRequestObjectResult)
            {
                (result.Result as BadRequestObjectResult)!.Should().NotBeNull();
            }
            else
            {
                var obj = result.Result as ObjectResult;
                obj.Should().NotBeNull();
                obj!.StatusCode.Should().Be(409);
            }
        }

        [Fact]
        public async Task Create_InvalidData_ReturnsBadRequest()
        {
            var dto = new CreateRolDto("", null);
            _mockSvc.Setup(s => s.CreateAsync(It.IsAny<CreateRolDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Datos inválidos"));

            var result = await _controller.Create(dto, CancellationToken.None);

            if (result.Result is BadRequestObjectResult)
            {
                (result.Result as BadRequestObjectResult)!.Should().NotBeNull();
            }
            else
            {
                var obj = result.Result as ObjectResult;
                obj.Should().NotBeNull();
                obj!.StatusCode.Should().Be(400);
            }
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithList()
        {
            var list = new List<RolDto>
            {
                new RolDto(1, "Admin", "Administrador")
            };
            _mockSvc.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(list);

            var action = await _controller.GetAll(CancellationToken.None);

            var ok = action.Result as OkObjectResult;
            ok.Should().NotBeNull();
            ok!.Value.Should().BeEquivalentTo(list);
        }

        [Fact]
        public async Task GetById_ExistingId_ReturnsOkWithItem()
        {
            var rol = new RolDto(1, "Admin", "Administrador");
            _mockSvc.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(rol);

            var action = await _controller.GetById(1, CancellationToken.None);

            var ok = action.Result as OkObjectResult;
            ok.Should().NotBeNull();
            ok!.Value.Should().BeEquivalentTo(rol);
        }

        [Fact]
        public async Task Update_Valid_ReturnsOk()
        {
            var updateDto = new UpdateRolDto("Admin", "Administrador");
            _mockSvc.Setup(s => s.UpdateAsync(1, It.IsAny<UpdateRolDto>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var action = await _controller.Update(1, updateDto, CancellationToken.None);

            var ok = action as OkObjectResult;
            ok.Should().NotBeNull();
            var statusProp = ok!.Value.GetType().GetProperty("status");
            statusProp.Should().NotBeNull();
            var statusValue = statusProp!.GetValue(ok.Value);
            statusValue.Should().Be(200);
        }

        [Fact]
        public async Task Delete_ExistingId_AcceptsNoContentOrOkWithStatus204()
        {
            _mockSvc.Setup(s => s.DeleteAsync(2, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var action = await _controller.Delete(2, CancellationToken.None);

            if (action is NoContentResult)
            {
                (action as NoContentResult)!.Should().NotBeNull();
            }
            else if (action is OkObjectResult ok)
            {
                var statusProp = ok.Value.GetType().GetProperty("status");
                statusProp.Should().NotBeNull();
                var statusValue = statusProp!.GetValue(ok.Value);
                statusValue.Should().Be(204);
            }
            else
            {
                action.Should().NotBeNull();
                throw new Xunit.Sdk.XunitException("Respuesta inesperada al eliminar: ni NoContent ni Ok(status:204)");
            }
        }

        [Fact]
        public async Task Delete_NotFoundId_AcceptsBadRequestOrNotFound()
        {
            _mockSvc.Setup(s => s.DeleteAsync(50, It.IsAny<CancellationToken>())).ThrowsAsync(new KeyNotFoundException());

            var action = await _controller.Delete(50, CancellationToken.None);

            if (action is BadRequestObjectResult)
            {
                (action as BadRequestObjectResult)!.Should().NotBeNull();
            }
            else if (action is NotFoundObjectResult)
            {
                (action as NotFoundObjectResult)!.Should().NotBeNull();
            }
            else
            {
                action.Should().NotBeNull();
                throw new Xunit.Sdk.XunitException("Respuesta inesperada al eliminar id no existente: ni BadRequest ni NotFound");
            }
        }
    }
}
