using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendIntegrador.Api.Controllers;
using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using BackendIntegrador.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace BackendIntegrador.Tests
{
    public class ParametrosCalidadControllerUnitTests
    {
        private readonly Mock<ICrudService<ParametroCalidadDto, CreateParametroCalidadDto, UpdateParametroCalidadDto>> _mockSvc;
        private readonly ParametrosCalidadController _controller;

        public ParametrosCalidadControllerUnitTests()
        {
            _mockSvc = new Mock<ICrudService<ParametroCalidadDto, CreateParametroCalidadDto, UpdateParametroCalidadDto>>();
            _controller = new ParametrosCalidadController(_mockSvc.Object, null!);
        }

        [Fact]
        public async Task Create_ConflictOrBadRequest()
        {
            var dto = new CreateParametroCalidadDto(null, "Param", null, null, null, null, 0);
            _mockSvc.Setup(s => s.CreateAsync(It.IsAny<CreateParametroCalidadDto>(), It.IsAny<CancellationToken>())).ThrowsAsync(new System.InvalidOperationException("Existe"));
            var result = await _controller.Create(dto, CancellationToken.None);
            if (result.Result is BadRequestObjectResult) (result.Result as BadRequestObjectResult)!.Should().NotBeNull(); else { var obj = result.Result as ObjectResult; obj.Should().NotBeNull(); obj!.StatusCode.Should().Be(409); }
        }

        [Fact]
        public async Task Create_InvalidData_ReturnsBadRequest()
        {
            var dto = new CreateParametroCalidadDto(null, "", null, 0m, 0m, null, 0);
            _mockSvc.Setup(s => s.CreateAsync(It.IsAny<CreateParametroCalidadDto>(), It.IsAny<CancellationToken>())).ThrowsAsync(new System.InvalidOperationException("Datos inválidos"));
            var result = await _controller.Create(dto, CancellationToken.None);
            if (result.Result is BadRequestObjectResult) (result.Result as BadRequestObjectResult)!.Should().NotBeNull(); else { var obj = result.Result as ObjectResult; obj.Should().NotBeNull(); obj!.StatusCode.Should().Be(400); }
        }

        [Fact]
        public async Task GetAll_ReturnsOk()
        {
            var list = new List<ParametroCalidadDto>{ new ParametroCalidadDto(1, null, "Param", null, null, null, null, 0) };
            _mockSvc.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(list);
            var action = await _controller.GetAll(CancellationToken.None);
            var ok = action.Result as OkObjectResult; ok.Should().NotBeNull(); ok!.Value.Should().BeEquivalentTo(list);
        }

        [Fact]
        public async Task GetById_ReturnsOk()
        {
            var item = new ParametroCalidadDto(1, null, "Param", null, null, null, null, 0);
            _mockSvc.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(item);
            var action = await _controller.GetById(1, CancellationToken.None);
            var ok = action.Result as OkObjectResult; ok.Should().NotBeNull(); ok!.Value.Should().BeEquivalentTo(item);
        }

        [Fact]
        public async Task Update_ReturnsOk()
        {
            var dto = new UpdateParametroCalidadDto("Param", null, null, null, null, 0);
            _mockSvc.Setup(s => s.UpdateAsync(1, It.IsAny<UpdateParametroCalidadDto>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            var action = await _controller.Update(1, dto, CancellationToken.None);
            var ok = action as OkObjectResult; ok.Should().NotBeNull();
        }

        [Fact]
        public async Task Delete_AcceptsNoContentOrOkWithStatus204()
        {
            _mockSvc.Setup(s => s.DeleteAsync(2, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            var action = await _controller.Delete(2, CancellationToken.None);
            if (action is NoContentResult) (action as NoContentResult)!.Should().NotBeNull(); else if (action is OkObjectResult ok) { var statusProp = ok.Value.GetType().GetProperty("status"); statusProp.Should().NotBeNull(); statusProp!.GetValue(ok.Value).Should().Be(204); } else throw new Xunit.Sdk.XunitException("Respuesta inesperada al eliminar");
        }

        [Fact]
        public async Task Delete_NotFoundOrBadRequest()
        {
            _mockSvc.Setup(s => s.DeleteAsync(50, It.IsAny<CancellationToken>())).ThrowsAsync(new KeyNotFoundException());
            var action = await _controller.Delete(50, CancellationToken.None);
            if (action is BadRequestObjectResult) (action as BadRequestObjectResult)!.Should().NotBeNull(); else if (action is NotFoundObjectResult) (action as NotFoundObjectResult)!.Should().NotBeNull(); else throw new Xunit.Sdk.XunitException("Respuesta inesperada al eliminar id no existente");
        }
    }
}
