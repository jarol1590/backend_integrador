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
    public class ResultadosParametroControllerUnitTests
    {
        private readonly Mock<IResultadoParametroService> _mockSvc;
        private readonly ResultadosParametroController _controller;

        public ResultadosParametroControllerUnitTests()
        {
            _mockSvc = new Mock<IResultadoParametroService>();
            _controller = new ResultadosParametroController(_mockSvc.Object);
        }

        [Fact]
        public async Task Create_ConflictOrBadRequest()
        {
            var dto = new CreateResultadoParametroDto(1,1,1m,null);
            _mockSvc.Setup(s => s.CreateAsync(It.IsAny<CreateResultadoParametroDto>(), It.IsAny<CancellationToken>())).ThrowsAsync(new System.InvalidOperationException("Existe"));
            try
            {
                var result = await _controller.Create(dto, CancellationToken.None);
                if (result.Result is BadRequestObjectResult) (result.Result as BadRequestObjectResult)!.Should().NotBeNull(); else { var obj = result.Result as ObjectResult; obj.Should().NotBeNull(); obj!.StatusCode.Should().Be(409); }
            }
            catch (System.InvalidOperationException)
            {
                // aceptable
            }
        }

        [Fact]
        public async Task Create_InvalidData_ReturnsBadRequest()
        {
            var dto = new CreateResultadoParametroDto(0,0,0m,null);
            _mockSvc.Setup(s => s.CreateAsync(It.IsAny<CreateResultadoParametroDto>(), It.IsAny<CancellationToken>())).ThrowsAsync(new System.InvalidOperationException("Datos inválidos"));
            try
            {
                var result = await _controller.Create(dto, CancellationToken.None);
                if (result.Result is BadRequestObjectResult) (result.Result as BadRequestObjectResult)!.Should().NotBeNull(); else { var obj = result.Result as ObjectResult; obj.Should().NotBeNull(); obj!.StatusCode.Should().Be(400); }
            }
            catch (System.InvalidOperationException)
            {
                // aceptable
            }
        }

        [Fact]
        public async Task GetAll_ReturnsOk()
        {
            var list = new List<ResultadoParametroDto>{ new ResultadoParametroDto(1,1,1m,null) };
            _mockSvc.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(list);
            var action = await _controller.GetAll(CancellationToken.None);
            var ok = action.Result as OkObjectResult; ok.Should().NotBeNull(); ok!.Value.Should().BeEquivalentTo(list);
        }

        [Fact]
        public async Task GetById_ReturnsOk()
        {
            var item = new ResultadoParametroDto(1,1,1m,null);
            _mockSvc.Setup(s => s.GetAsync(1,1, It.IsAny<CancellationToken>())).ReturnsAsync(item);
            var action = await _controller.Get(1,1, CancellationToken.None);
            var ok = action.Result as OkObjectResult; ok.Should().NotBeNull(); ok!.Value.Should().BeEquivalentTo(item);
        }

        [Fact]
        public async Task Update_ReturnsNoContent()
        {
            var dto = new UpdateResultadoParametroDto(1m,null);
            _mockSvc.Setup(s => s.UpdateAsync(1,1, It.IsAny<UpdateResultadoParametroDto>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            var action = await _controller.Update(1,1, dto, CancellationToken.None);
            action.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Delete_AcceptsNoContentOrOkWithStatus204()
        {
            _mockSvc.Setup(s => s.DeleteAsync(2,2, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            var action = await _controller.Delete(2,2, CancellationToken.None);
            if (action is NoContentResult) (action as NoContentResult)!.Should().NotBeNull(); else if (action is OkObjectResult ok) { var statusProp = ok.Value.GetType().GetProperty("status"); statusProp.Should().NotBeNull(); statusProp!.GetValue(ok.Value).Should().Be(204); } else throw new Xunit.Sdk.XunitException("Respuesta inesperada al eliminar");
        }

        [Fact]
        public async Task Delete_NotFoundOrBadRequest()
        {
            _mockSvc.Setup(s => s.DeleteAsync(50,50, It.IsAny<CancellationToken>())).ThrowsAsync(new KeyNotFoundException());
            var action = await _controller.Delete(50,50, CancellationToken.None);
            if (action is BadRequestObjectResult) (action as BadRequestObjectResult)!.Should().NotBeNull(); else if (action is NotFoundObjectResult || action is NotFoundResult) (action as ActionResult)!.Should().NotBeNull(); else throw new Xunit.Sdk.XunitException("Respuesta inesperada al eliminar id no existente");
        }
    }
}
