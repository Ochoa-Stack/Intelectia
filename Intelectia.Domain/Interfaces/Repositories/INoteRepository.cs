using Intelectia.Domain.Entities;

namespace Intelectia.Domain.Interfaces.Repositories;

public interface INoteRepository
{
    // Trae todas las notas del usuario; si bookId tiene valor, filtra por ese libro
    Task<IReadOnlyList<Note>> GetByUserIdAsync(
        Guid userId, Guid? bookId, CancellationToken cancellationToken = default);

    // Busca una nota por ID; null si no existe o fue eliminada
    Task<Note?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Agrega una nota nueva
    Task AddAsync(Note note, CancellationToken cancellationToken = default);

    // Marca la nota como modificada
    void Update(Note note);

    // Aplica soft delete a la nota
    void Delete(Note note);
}
