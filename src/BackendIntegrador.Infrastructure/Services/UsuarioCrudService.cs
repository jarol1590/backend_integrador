using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using BackendIntegrador.Domain.Entities;

namespace BackendIntegrador.Infrastructure.Services;

internal sealed class UsuarioCrudService : ICrudService<UsuarioDto, CreateUsuarioDto, UpdateUsuarioDto>
{
    private readonly IRepository<Usuario> _repo;

    public UsuarioCrudService(IRepository<Usuario> repo) => _repo = repo;

    public async Task<IReadOnlyList<UsuarioDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await _repo.GetAllAsync(cancellationToken);
        return list.Select(u => new UsuarioDto(u.UsuarioId, u.Email, u.Estado, u.FechaCreacion, u.CentroAcopioId)).ToList();
    }

    public async Task<UsuarioDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var u = await _repo.FindAsync(new object[] { id }, cancellationToken);
        return u is null ? null : new UsuarioDto(u.UsuarioId, u.Email, u.Estado, u.FechaCreacion, u.CentroAcopioId);
    }

    public async Task<UsuarioDto> CreateAsync(CreateUsuarioDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new Usuario
        {
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Estado = dto.Estado,
            FechaCreacion = dto.FechaCreacion ?? DateTime.UtcNow,
            CentroAcopioId = dto.CentroAcopioId,
        };
        await _repo.AddAsync(entity, cancellationToken);
        return new UsuarioDto(entity.UsuarioId, entity.Email, entity.Estado, entity.FechaCreacion, entity.CentroAcopioId);
    }

    public async Task UpdateAsync(int id, UpdateUsuarioDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _repo.FindAsync(new object[] { id }, cancellationToken);
        if (entity is null)
            throw new KeyNotFoundException($"Usuario con id {id} no existe.");
        entity.Email = dto.Email;
        entity.Estado = dto.Estado;
        entity.CentroAcopioId = dto.CentroAcopioId;
        if (!string.IsNullOrWhiteSpace(dto.Password))
            entity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        await _repo.UpdateAsync(entity, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repo.FindAsync(new object[] { id }, cancellationToken);
        if (entity is null)
            throw new KeyNotFoundException($"Usuario con id {id} no existe.");
        await _repo.RemoveAsync(entity, cancellationToken);
    }
}
