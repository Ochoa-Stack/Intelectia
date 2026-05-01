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

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
