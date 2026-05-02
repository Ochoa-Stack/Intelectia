using Intelectia.Domain.Entities;

namespace Intelectia.Domain.Interfaces.Repositories;

public interface ICartRepository
{
    // Busca el carrito del usuario con todos sus items y libros cargados
    Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    // Crea un carrito nuevo para el usuario
    Task AddAsync(Cart cart, CancellationToken cancellationToken = default);

    // Marca el carrito como modificado
    void Update(Cart cart);
}
