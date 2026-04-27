using BackendIntegrador.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BackendIntegrador.Infrastructure.Persistence;

public sealed class EfRepository<TEntity> : IRepository<TEntity> where TEntity : class
{
    private readonly AppDbContext _db;

    public EfRepository(AppDbContext db) => _db = db;

    public Task<TEntity?> FindAsync(object?[]? keyValues, CancellationToken cancellationToken = default) =>
        _db.Set<TEntity>().FindAsync(keyValues ?? Array.Empty<object?>(), cancellationToken).AsTask();

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _db.Set<TEntity>().AsNoTracking().ToListAsync(cancellationToken);

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _db.Set<TEntity>().AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _db.Set<TEntity>().Update(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _db.Set<TEntity>().Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
