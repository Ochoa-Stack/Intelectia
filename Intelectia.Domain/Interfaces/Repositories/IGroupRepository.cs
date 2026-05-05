using Intelectia.Domain.Entities;

namespace Intelectia.Domain.Interfaces.Repositories;

public interface IGroupRepository
{
    // Trae los grupos a los que pertenece el usuario
    Task<IReadOnlyList<StudyGroup>> GetByUserIdAsync(
        Guid userId, CancellationToken cancellationToken = default);

    // Trae los grupos públicos para explorar, excluyendo los que ya pertenece el usuario
    Task<IReadOnlyList<StudyGroup>> GetPublicGroupsAsync(
        Guid userId, string? search, CancellationToken cancellationToken = default);

    // Trae un grupo por ID con sus miembros
    Task<StudyGroup?> GetByIdWithMembersAsync(
        Guid id, CancellationToken cancellationToken = default);

    // Agrega un grupo nuevo
    Task AddAsync(StudyGroup group, CancellationToken cancellationToken = default);

    // Marca el grupo como modificado
    void Update(StudyGroup group);
}
