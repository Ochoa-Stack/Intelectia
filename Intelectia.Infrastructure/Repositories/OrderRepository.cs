using Microsoft.EntityFrameworkCore;
using Intelectia.Domain.Entities;
using Intelectia.Domain.Interfaces.Repositories;
using Intelectia.Infrastructure.Persistence;

namespace Intelectia.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    // Trae todos los pedidos del usuario ordenados del más reciente al más antiguo
    public async Task<IReadOnlyList<Order>> GetByUserIdAsync(
        Guid userId, CancellationToken cancellationToken = default)
        => await _context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Book)
            .Include(o => o.Payment)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

    // Carga el pedido completo con items, libros, categoría y pago para la vista de detalle
    public Task<Order?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Book)
                    .ThenInclude(b => b.Category)
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    // Busca el pedido usando el ID del PaymentIntent de Stripe; crítico para el webhook
    public Task<Order?> GetByPaymentIntentIdAsync(
        string paymentIntentId, CancellationToken cancellationToken = default)
        => _context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Book)
            .Include(o => o.Payment)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o =>
                o.Payment != null &&
                o.Payment.StripePaymentIntentId == paymentIntentId,
                cancellationToken);

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
        => await _context.Orders.AddAsync(order, cancellationToken);

    public void Update(Order order)
        => _context.Orders.Update(order);
}
