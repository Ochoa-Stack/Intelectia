using Intelectia.Domain.Entities;

namespace Intelectia.Domain.Interfaces.Repositories;

public interface ICitationRepository
{
    // Trae todas las citas del usuario; si bookId tiene valor, filtra por ese libro
    Task<IReadOnlyList<Citation>> GetByUserIdAsync(
        Guid userId, Guid? bookId, CancellationToken cancellationToken = default);

    // Busca una cita por ID; null si no existe o fue eliminada
    Task<Citation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Agrega una cita nueva
    Task AddAsync(Citation citation, CancellationToken cancellationToken = default);

    // Marca la cita como modificada
    void Update(Citation citation);

    // Aplica soft delete a la cita
    void Delete(Citation citation);
}
