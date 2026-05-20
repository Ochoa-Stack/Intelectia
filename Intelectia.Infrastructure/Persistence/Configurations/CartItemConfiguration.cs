using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Intelectia.Domain.Entities;

namespace Intelectia.Infrastructure.Persistence.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");
        builder.HasKey(ci => ci.Id);

        // Un libro no puede estar dos veces en el mismo carrito
        builder.HasIndex(ci => new { ci.CartId, ci.BookId }).IsUnique();

        // Precio capturado al momento de agregar al carrito
        builder.Property(ci => ci.PriceSnapshot).HasColumnType("numeric(18,2)");

        // Restrict para no eliminar CartItems si el libro se desactiva
        builder.HasOne(ci => ci.Book)
               .WithMany()
               .HasForeignKey(ci => ci.BookId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
