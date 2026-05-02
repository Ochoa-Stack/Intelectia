using Microsoft.EntityFrameworkCore;
using Intelectia.Domain.Entities;

namespace Intelectia.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    // Tablas de auth
    DbSet<RefreshToken> RefreshTokens { get; }

    // Tablas del Marketplace
    DbSet<Book> Books { get; }
    DbSet<Category> Categories { get; }
    DbSet<Review> Reviews { get; }
    DbSet<UserBook> UserBooks { get; }

    // Tablas de Comercio
    DbSet<Cart> Carts { get; }
    DbSet<CartItem> CartItems { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<Payment> Payments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
