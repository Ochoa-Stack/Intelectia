using Microsoft.EntityFrameworkCore;
using Intelectia.Domain.Entities;
using Intelectia.Domain.Interfaces.Repositories;
using Intelectia.Infrastructure.Persistence;

namespace Intelectia.Infrastructure.Repositories;

public class GroupMessageRepository : IGroupMessageRepository
{
    private readonly AppDbContext _context;

    public GroupMessageRepository(AppDbContext context)
    {
        _context = context;
    }

    // Trae los mensajes paginados del más reciente al más antiguo
    public async Task<(IReadOnlyList<GroupMessage> Items, int TotalCount)> GetPagedByGroupIdAsync(
        Guid groupId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.GroupMessages
            .Include(m => m.User)
            .Where(m => m.GroupId == groupId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(GroupMessage message, CancellationToken cancellationToken = default)
        => await _context.GroupMessages.AddAsync(message, cancellationToken);
}
