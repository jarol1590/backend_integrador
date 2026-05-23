using BackendIntegrador.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendIntegrador.Api.Controllers;

[Authorize]
public abstract class IntKeyCrudControllerBase<TRead, TCreate, TUpdate> : ControllerBase
    where TRead : class
{
    private readonly ICrudService<TRead, TCreate, TUpdate> _svc;
    private readonly Func<TRead, int> _readId;

    protected IntKeyCrudControllerBase(ICrudService<TRead, TCreate, TUpdate> svc, Func<TRead, int> readId)
    {
        _svc = svc;
        _readId = readId;
    }

    [HttpGet]
    public virtual async Task<ActionResult<IReadOnlyList<TRead>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _svc.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public virtual async Task<ActionResult<TRead>> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await _svc.GetByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public virtual async Task<ActionResult<TRead>> Create([FromBody] TCreate dto, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _svc.CreateAsync(dto, cancellationToken);
            var response = new
            {
                message = "Creación exitosa",
                data = created,
                status = 201
            };
            return CreatedAtAction(nameof(GetById), new { id = _readId(created) }, response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message,
                status = 400
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Error interno del servidor",
                status = ex.Message
            });
        }
    }

    [HttpPut("{id:int}")]
    public virtual async Task<IActionResult> Update(int id, [FromBody] TUpdate dto, CancellationToken cancellationToken)
    {
        try
        {
            await _svc.UpdateAsync(id, dto, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id:int}")]
    public virtual async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _svc.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
