using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Intelectia.Domain.Entities;

namespace Intelectia.Infrastructure.Persistence.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");
        builder.HasKey(oi => oi.Id);

        // Precio inmutable al momento de la compra
        builder.Property(oi => oi.PriceSnapshot).HasColumnType("numeric(18,2)");

        // Restrict: no perder el historial de compras si el libro cambia
        builder.HasOne(oi => oi.Book)
               .WithMany()
               .HasForeignKey(oi => oi.BookId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
