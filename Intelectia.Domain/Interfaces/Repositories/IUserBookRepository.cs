using Intelectia.Domain.Entities;

namespace Intelectia.Domain.Interfaces.Repositories;

public interface IUserBookRepository
{
    // Trae los libros adquiridos por el usuario con detalles de libro y categoría
    Task<IReadOnlyList<UserBook>> GetUserBooksWithDetailsAsync(Guid userId, CancellationToken cancellationToken = default);
}
