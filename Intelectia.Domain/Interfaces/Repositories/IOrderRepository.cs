using Intelectia.Domain.Entities;

namespace Intelectia.Domain.Interfaces.Repositories;

public interface IOrderRepository
{
    // Trae todos los pedidos del usuario con sus items; para la pantalla de historial
    Task<IReadOnlyList<Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    // Trae un pedido por ID con items y pago; para la vista de detalle
    Task<Order?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    // Busca un pedido por el ID del PaymentIntent de Stripe; usado en el webhook
    Task<Order?> GetByPaymentIntentIdAsync(string paymentIntentId, CancellationToken cancellationToken = default);

    // Agrega un pedido nuevo
    Task AddAsync(Order order, CancellationToken cancellationToken = default);

    // Marca el pedido como modificado
    void Update(Order order);
}
