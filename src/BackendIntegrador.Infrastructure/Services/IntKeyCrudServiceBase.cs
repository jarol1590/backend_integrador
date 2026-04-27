using BackendIntegrador.Application.Abstractions;

namespace BackendIntegrador.Infrastructure.Services;

internal abstract class IntKeyCrudServiceBase<TEntity, TRead, TCreate, TUpdate> : ICrudService<TRead, TCreate, TUpdate>
    where TEntity : class, new()
{
    private readonly IRepository<TEntity> _repo;

    protected IntKeyCrudServiceBase(IRepository<TEntity> repo) => _repo = repo;

    protected abstract int GetId(TEntity entity);
    protected abstract TRead MapRead(TEntity entity);
    protected abstract TEntity MapCreate(TCreate dto);
    protected abstract void ApplyUpdate(TEntity entity, TUpdate dto);

    public virtual async Task<IReadOnlyList<TRead>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await _repo.GetAllAsync(cancellationToken);
        return list.Select(MapRead).ToList();
    }

    public virtual async Task<TRead?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repo.FindAsync(new object[] { id }, cancellationToken);
        return entity is null ? default : MapRead(entity);
    }

    public virtual async Task<TRead> CreateAsync(TCreate dto, CancellationToken cancellationToken = default)
    {
        var entity = MapCreate(dto);
        await _repo.AddAsync(entity, cancellationToken);
        return MapRead(entity);
    }

    public virtual async Task UpdateAsync(int id, TUpdate dto, CancellationToken cancellationToken = default)
    {
        var entity = await _repo.FindAsync(new object[] { id }, cancellationToken);
        if (entity is null)
            throw new KeyNotFoundException($"{typeof(TEntity).Name} con id {id} no existe.");
        ApplyUpdate(entity, dto);
        await _repo.UpdateAsync(entity, cancellationToken);
    }

    public virtual async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repo.FindAsync(new object[] { id }, cancellationToken);
        if (entity is null)
            throw new KeyNotFoundException($"{typeof(TEntity).Name} con id {id} no existe.");
        await _repo.RemoveAsync(entity, cancellationToken);
    }
}
