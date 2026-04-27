namespace BackendIntegrador.Application.Abstractions;

public interface IRepository<TEntity> where TEntity : class
{
    Task<TEntity?> FindAsync(object?[]? keyValues, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default);
}
