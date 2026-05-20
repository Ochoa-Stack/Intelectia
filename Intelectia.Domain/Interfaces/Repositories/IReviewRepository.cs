using Intelectia.Domain.Entities;

namespace Intelectia.Domain.Interfaces.Repositories;

public interface IReviewRepository
{
    // Trae una reseña por su Id con el usuario que la creó
    Task<Review?> GetByIdWithUserAsync(Guid id, CancellationToken cancellationToken = default);

    // Busca una reseña específica de un usuario para un libro
    Task<Review?> GetReviewByUserAndBookAsync(Guid userId, Guid bookId, CancellationToken cancellationToken = default);

    // Añade una reseña
    Task AddAsync(Review review, CancellationToken cancellationToken = default);
}
