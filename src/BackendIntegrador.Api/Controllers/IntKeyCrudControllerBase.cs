using BackendIntegrador.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace BackendIntegrador.Api.Controllers;

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
    public async Task<ActionResult<IReadOnlyList<TRead>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _svc.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TRead>> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await _svc.GetByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<TRead>> Create([FromBody] TCreate dto, CancellationToken cancellationToken)
    {
        var created = await _svc.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = _readId(created) }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] TUpdate dto, CancellationToken cancellationToken)
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
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
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
