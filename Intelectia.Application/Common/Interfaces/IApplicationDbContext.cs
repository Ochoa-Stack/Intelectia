using Microsoft.EntityFrameworkCore;
using Intelectia.Domain.Entities;

namespace Intelectia.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    // Tablas de auth y perfiles
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<User> Users { get; }
    DbSet<VendorProfile> VendorProfiles { get; }

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

    // Herramientas de estudio
    DbSet<Note> Notes { get; }
    DbSet<Citation> Citations { get; }
    DbSet<TranslationHistory> TranslationHistories { get; }

    // Grupos de Estudio
    DbSet<StudyGroup> StudyGroups { get; }
    DbSet<GroupMember> GroupMembers { get; }
    DbSet<GroupMessage> GroupMessages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
