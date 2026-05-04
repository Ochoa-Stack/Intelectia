using Intelectia.Domain.Entities;

namespace Intelectia.Domain.Interfaces.Repositories;

public interface IGroupMessageRepository
{
    // Trae los mensajes de un grupo con paginación; del más reciente al más antiguo
    Task<(IReadOnlyList<GroupMessage> Items, int TotalCount)> GetPagedByGroupIdAsync(
        Guid groupId, int page, int pageSize,
        CancellationToken cancellationToken = default);

    // Agrega un mensaje nuevo
    Task AddAsync(GroupMessage message, CancellationToken cancellationToken = default);
}
