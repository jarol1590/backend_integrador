using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using BackendIntegrador.Domain.Entities;

namespace BackendIntegrador.Infrastructure.Services;

internal sealed class UsuarioRolService : IUsuarioRolService
{
    private readonly IRepository<UsuarioRol> _repo;

    public UsuarioRolService(IRepository<UsuarioRol> repo) => _repo = repo;

    public async Task<IReadOnlyList<UsuarioRolDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await _repo.GetAllAsync(cancellationToken);
        return list.Select(ur => new UsuarioRolDto(ur.UsuarioId, ur.RolId)).ToList();
    }

    public async Task<UsuarioRolDto?> GetAsync(int usuarioId, int rolId, CancellationToken cancellationToken = default)
    {
        var ur = await _repo.FindAsync(new object[] { usuarioId, rolId }, cancellationToken);
        return ur is null ? null : new UsuarioRolDto(ur.UsuarioId, ur.RolId);
    }

    public async Task<UsuarioRolDto> CreateAsync(CreateUsuarioRolDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new UsuarioRol { UsuarioId = dto.UsuarioId, RolId = dto.RolId };
        await _repo.AddAsync(entity, cancellationToken);
        return new UsuarioRolDto(entity.UsuarioId, entity.RolId);
    }

    public async Task DeleteAsync(int usuarioId, int rolId, CancellationToken cancellationToken = default)
    {
        var entity = await _repo.FindAsync(new object[] { usuarioId, rolId }, cancellationToken);
        if (entity is null)
            throw new KeyNotFoundException($"UsuarioRol ({usuarioId},{rolId}) no existe.");
        await _repo.RemoveAsync(entity, cancellationToken);
    }
}

internal sealed class ResultadoParametroService : IResultadoParametroService
{
    private readonly IRepository<ResultadoParametro> _repo;

    public ResultadoParametroService(IRepository<ResultadoParametro> repo) => _repo = repo;

    public async Task<IReadOnlyList<ResultadoParametroDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await _repo.GetAllAsync(cancellationToken);
        return list.Select(r => new ResultadoParametroDto(r.AnalisisId, r.ParametroId, r.ValorResultado, r.Observacion)).ToList();
    }

    public async Task<ResultadoParametroDto?> GetAsync(int analisisId, int parametroId, CancellationToken cancellationToken = default)
    {
        var r = await _repo.FindAsync(new object[] { analisisId, parametroId }, cancellationToken);
        return r is null ? null : new ResultadoParametroDto(r.AnalisisId, r.ParametroId, r.ValorResultado, r.Observacion);
    }

    public async Task<ResultadoParametroDto> CreateAsync(CreateResultadoParametroDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new ResultadoParametro
        {
            AnalisisId = dto.AnalisisId,
            ParametroId = dto.ParametroId,
            ValorResultado = dto.ValorResultado,
            Observacion = dto.Observacion,
        };
        await _repo.AddAsync(entity, cancellationToken);
        return new ResultadoParametroDto(entity.AnalisisId, entity.ParametroId, entity.ValorResultado, entity.Observacion);
    }

    public async Task UpdateAsync(int analisisId, int parametroId, UpdateResultadoParametroDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _repo.FindAsync(new object[] { analisisId, parametroId }, cancellationToken);
        if (entity is null)
            throw new KeyNotFoundException($"ResultadoParametro ({analisisId},{parametroId}) no existe.");
        entity.ValorResultado = dto.ValorResultado;
        entity.Observacion = dto.Observacion;
        await _repo.UpdateAsync(entity, cancellationToken);
    }

    public async Task DeleteAsync(int analisisId, int parametroId, CancellationToken cancellationToken = default)
    {
        var entity = await _repo.FindAsync(new object[] { analisisId, parametroId }, cancellationToken);
        if (entity is null)
            throw new KeyNotFoundException($"ResultadoParametro ({analisisId},{parametroId}) no existe.");
        await _repo.RemoveAsync(entity, cancellationToken);
    }
}
