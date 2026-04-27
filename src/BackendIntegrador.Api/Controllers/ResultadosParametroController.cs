using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace BackendIntegrador.Api.Controllers;

[ApiController]
[Route("api/resultados-parametro")]
public sealed class ResultadosParametroController : ControllerBase
{
    private readonly IResultadoParametroService _svc;

    public ResultadosParametroController(IResultadoParametroService svc) => _svc = svc;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ResultadoParametroDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _svc.GetAllAsync(cancellationToken));

    [HttpGet("{analisisId:int}/{parametroId:int}")]
    public async Task<ActionResult<ResultadoParametroDto>> Get(int analisisId, int parametroId, CancellationToken cancellationToken)
    {
        var item = await _svc.GetAsync(analisisId, parametroId, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<ResultadoParametroDto>> Create([FromBody] CreateResultadoParametroDto dto, CancellationToken cancellationToken)
    {
        var created = await _svc.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(Get), new { analisisId = created.AnalisisId, parametroId = created.ParametroId }, created);
    }

    [HttpPut("{analisisId:int}/{parametroId:int}")]
    public async Task<IActionResult> Update(int analisisId, int parametroId, [FromBody] UpdateResultadoParametroDto dto, CancellationToken cancellationToken)
    {
        try
        {
            await _svc.UpdateAsync(analisisId, parametroId, dto, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{analisisId:int}/{parametroId:int}")]
    public async Task<IActionResult> Delete(int analisisId, int parametroId, CancellationToken cancellationToken)
    {
        try
        {
            await _svc.DeleteAsync(analisisId, parametroId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
