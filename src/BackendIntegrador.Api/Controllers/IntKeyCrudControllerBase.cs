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

    protected virtual string GetCreateSuccessMessage() => "Registro creado correctamente";

    protected virtual string GetUpdateSuccessMessage() => "Registro actualizado correctamente";

    protected virtual string GetDeleteSuccessMessage() => "Registro eliminado correctamente";


    [HttpGet]
    public virtual async Task<ActionResult<IReadOnlyList<TRead>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _svc.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public virtual async Task<ActionResult<TRead>> GetById(int id, CancellationToken cancellationToken)
    {
        try        {
            var item = await _svc.GetByIdAsync(id, cancellationToken);
            return item is null ? NotFound() : Ok(item);
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

    [HttpPost]
    public virtual async Task<ActionResult<TRead>> Create([FromBody] TCreate dto, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _svc.CreateAsync(dto, cancellationToken);
            var response = new
            {
                message = GetCreateSuccessMessage(),
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

            var response = new
            {
                message = GetUpdateSuccessMessage(),
                status = 200
            };
            return Ok(response);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new
            {
                message = "Registro no encontrado",
                status = 404
            });
        }catch (InvalidOperationException ex)
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

    [HttpDelete("{id:int}")]
    public virtual async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _svc.DeleteAsync(id, cancellationToken);
            var response = new
            {
                message = GetDeleteSuccessMessage(),
                status = 204
            };
            return Ok(response);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new
            {
                message = "Registro no encontrado",
                status = 404
            });
        }catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Error interno del servidor",
                status = ex.Message
            });
        }
    }
}
