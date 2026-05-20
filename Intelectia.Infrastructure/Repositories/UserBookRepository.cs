using Microsoft.EntityFrameworkCore;
using Intelectia.Domain.Entities;
using Intelectia.Domain.Interfaces.Repositories;
using Intelectia.Infrastructure.Persistence;

namespace Intelectia.Infrastructure.Repositories;

public class UserBookRepository : IUserBookRepository
{
    private readonly AppDbContext _context;

    public UserBookRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<UserBook>> GetUserBooksWithDetailsAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserBooks
            .Include(ub => ub.Book)
                .ThenInclude(b => b.Category)
            .Where(ub => ub.UserId == userId && !ub.IsDeleted)
            .OrderByDescending(ub => ub.AcquiredAt)
            .ToListAsync(cancellationToken);
    }
}
