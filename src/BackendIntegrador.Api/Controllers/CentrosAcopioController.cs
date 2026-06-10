using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using BackendIntegrador.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendIntegrador.Api.Controllers;

[ApiController]
[Route("api/centros-acopio")]
public sealed class CentrosAcopioController : IntKeyCrudControllerBase<CentroAcopioDto, CreateCentroAcopioDto, UpdateCentroAcopioDto>
{
    private readonly AppDbContext _db;

    public CentrosAcopioController(
        ICrudService<CentroAcopioDto, CreateCentroAcopioDto, UpdateCentroAcopioDto> svc,
        AppDbContext db)
        : base(svc, c => c.CentroAcopioId)
    {
        _db = db;
    }

    [AllowAnonymous]
    [HttpGet]
    public override async Task<ActionResult<IReadOnlyList<CentroAcopioDto>>> GetAll(CancellationToken cancellationToken)
        => await base.GetAll(cancellationToken);

    [AllowAnonymous]
    [HttpGet("{id:int}/trabajadores")]
    public async Task<ActionResult<IReadOnlyList<object>>> GetTrabajadores(int id, CancellationToken cancellationToken)
    {
        var trabajadores = await _db.Usuarios
            .AsNoTracking()
            .Where(u => u.CentroAcopioId == id && u.Trabajador != null)
            .Select(u => new
            {
                u.UsuarioId,
                u.Email,
                u.Trabajador!.Nombre,
                u.Trabajador.Documento,
                u.Trabajador.Telefono,
            })
            .ToListAsync(cancellationToken);

        return Ok(trabajadores as IReadOnlyList<object>);
    }
}
