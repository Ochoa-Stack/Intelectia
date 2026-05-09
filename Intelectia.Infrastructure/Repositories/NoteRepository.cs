using Microsoft.EntityFrameworkCore;
using Intelectia.Domain.Entities;
using Intelectia.Domain.Interfaces.Repositories;
using Intelectia.Infrastructure.Persistence;

namespace Intelectia.Infrastructure.Repositories;

public class NoteRepository : INoteRepository
{
    private readonly AppDbContext _context;

    public NoteRepository(AppDbContext context)
    {
        _context = context;
    }

    // Trae las notas del usuario ordenadas por última modificación
    // Si se pasa bookId filtra solo las de ese libro
    public async Task<IReadOnlyList<Note>> GetByUserIdAsync(
        Guid userId, Guid? bookId, CancellationToken cancellationToken = default)
    {
        var query = _context.Notes
            .Include(n => n.Book)
            .Where(n => n.UserId == userId);

        if (bookId.HasValue)
            query = query.Where(n => n.BookId == bookId.Value);

        return await query
            .OrderByDescending(n => n.UpdatedAt ?? n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<Note?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Notes
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

    public async Task AddAsync(Note note, CancellationToken cancellationToken = default)
        => await _context.Notes.AddAsync(note, cancellationToken);

    public void Update(Note note)
        => _context.Notes.Update(note);

    public void Delete(Note note)
    {
        note.IsDeleted = true;
        note.DeletedAt = DateTime.UtcNow;
        _context.Notes.Update(note);
    }
}
