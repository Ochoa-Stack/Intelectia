using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Intelectia.Domain.Entities;

namespace Intelectia.Infrastructure.Persistence.Configurations;

public class UserBookConfiguration : IEntityTypeConfiguration<UserBook>
{
    public void Configure(EntityTypeBuilder<UserBook> builder)
    {
        builder.ToTable("UserBooks");
        builder.HasKey(ub => ub.Id);

        // Un usuario no puede tener el mismo libro dos veces
        builder.HasIndex(ub => new { ub.UserId, ub.BookId }).IsUnique();

        // Precisión para el porcentaje de progreso (0.00 – 100.00)
        builder.Property(ub => ub.ReadingProgress).HasColumnType("decimal(5,2)");

        builder.HasOne(ub => ub.User)
               .WithMany(u => u.UserBooks)
               .HasForeignKey(ub => ub.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ub => ub.Book)
               .WithMany(b => b.UserBooks)
               .HasForeignKey(ub => ub.BookId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
