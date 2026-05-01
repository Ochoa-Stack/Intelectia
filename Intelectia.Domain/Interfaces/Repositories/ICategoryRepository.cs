using Intelectia.Domain.Entities;

namespace Intelectia.Domain.Interfaces.Repositories;

public interface ICategoryRepository
{
    // Trae todas las categorías activas para los filtros del catálogo
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default);

    // Busca una categoría por su ID
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
