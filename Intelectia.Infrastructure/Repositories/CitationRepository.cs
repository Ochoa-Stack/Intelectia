using Microsoft.EntityFrameworkCore;
using Intelectia.Domain.Entities;
using Intelectia.Domain.Interfaces.Repositories;
using Intelectia.Infrastructure.Persistence;

namespace Intelectia.Infrastructure.Repositories;

public class CitationRepository : ICitationRepository
{
    private readonly AppDbContext _context;

    public CitationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Citation>> GetByUserIdAsync(
        Guid userId, Guid? bookId, CancellationToken cancellationToken = default)
    {
        var query = _context.Citations
            .Include(c => c.Book)
            .Where(c => c.UserId == userId);

        if (bookId.HasValue)
            query = query.Where(c => c.BookId == bookId.Value);

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<Citation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Citations
            .Include(c => c.Book)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task AddAsync(Citation citation, CancellationToken cancellationToken = default)
        => await _context.Citations.AddAsync(citation, cancellationToken);

    public void Update(Citation citation)
        => _context.Citations.Update(citation);

    public void Delete(Citation citation)
    {
        citation.IsDeleted = true;
        citation.DeletedAt = DateTime.UtcNow;
        _context.Citations.Update(citation);
    }
}
