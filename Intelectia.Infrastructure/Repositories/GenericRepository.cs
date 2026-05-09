using Microsoft.EntityFrameworkCore;
using Intelectia.Domain.Common;
using Intelectia.Domain.Interfaces.Repositories;
using Intelectia.Infrastructure.Persistence;

namespace Intelectia.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbSet.ToListAsync(cancellationToken);

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        => await _dbSet.AddAsync(entity, cancellationToken);

    public void Update(T entity)
        => _dbSet.Update(entity);

    public void SoftDelete(T entity)
    {
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
    }
}
