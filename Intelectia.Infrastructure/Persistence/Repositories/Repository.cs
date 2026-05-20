using System.Linq.Expressions;
using Intelectia.Domain.Common;
using Intelectia.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Intelectia.Infrastructure.Persistence.Repositories;

// Implementamos la lógica de acceso a datos genérica
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public IQueryable<T> GetAll() => _dbSet.AsNoTracking();

    public IQueryable<T> Find(Expression<Func<T, bool>> predicate) => _dbSet.Where(predicate);

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public void Delete(T entity)
    {
        // Marcamos la entidad para borrado lógico
        entity.IsDeleted = true;
        _dbSet.Update(entity);
    }
}
