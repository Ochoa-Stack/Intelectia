using Microsoft.EntityFrameworkCore;
using Intelectia.Domain.Entities;
using Intelectia.Domain.Interfaces.Repositories;
using Intelectia.Infrastructure.Persistence;

namespace Intelectia.Infrastructure.Repositories;

public class GroupRepository : IGroupRepository
{
    private readonly AppDbContext _context;

    public GroupRepository(AppDbContext context)
    {
        _context = context;
    }

    // Trae los grupos donde el usuario es miembro activo
    public async Task<IReadOnlyList<StudyGroup>> GetByUserIdAsync(
        Guid userId, CancellationToken cancellationToken = default)
        => await _context.StudyGroups
            .Include(g => g.Members.Where(m => !m.IsDeleted))
                .ThenInclude(m => m.User)
            .Where(g => !g.IsDeleted &&
                        g.Members.Any(m => m.UserId == userId && !m.IsDeleted))
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync(cancellationToken);

    // Trae grupos públicos excluyendo los que el usuario ya integra
    public async Task<IReadOnlyList<StudyGroup>> GetPublicGroupsAsync(
        Guid userId, string? search, CancellationToken cancellationToken = default)
    {
        var query = _context.StudyGroups
            .Include(g => g.Members.Where(m => !m.IsDeleted))
            .Where(g => !g.IsDeleted &&
                        g.IsPublic &&
                        !g.Members.Any(m => m.UserId == userId && !m.IsDeleted));

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(g => g.Name.ToLower().Contains(search.ToLower()));

        return await query
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    // Carga el grupo con sus miembros para operaciones de gestión
    public Task<StudyGroup?> GetByIdWithMembersAsync(
        Guid id, CancellationToken cancellationToken = default)
        => _context.StudyGroups
            .Include(g => g.Members.Where(m => !m.IsDeleted))
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted, cancellationToken);

    public async Task AddAsync(StudyGroup group, CancellationToken cancellationToken = default)
        => await _context.StudyGroups.AddAsync(group, cancellationToken);

    public void Update(StudyGroup group)
        => _context.StudyGroups.Update(group);
}
