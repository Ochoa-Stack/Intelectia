using Microsoft.EntityFrameworkCore;
using Intelectia.Domain.Entities;
using Intelectia.Domain.Interfaces.Repositories;
using Intelectia.Infrastructure.Persistence;

namespace Intelectia.Infrastructure.Repositories;

public class CartRepository : ICartRepository
{
    private readonly AppDbContext _context;

    public CartRepository(AppDbContext context)
    {
        _context = context;
    }

    // Carga el carrito con todos sus items y los datos del libro para calcular Cart.Total
    public Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => _context.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Book)
                    .ThenInclude(b => b.Category)
            .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted, cancellationToken);

    public async Task AddAsync(Cart cart, CancellationToken cancellationToken = default)
        => await _context.Carts.AddAsync(cart, cancellationToken);

    public void Update(Cart cart)
        => _context.Carts.Update(cart);
}
