using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Intelectia.Domain.Entities;

namespace Intelectia.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.StripePaymentIntentId).IsRequired().HasMaxLength(200);
        builder.Property(p => p.StripeClientSecret).IsRequired().HasMaxLength(500);

        // Índice único para buscar el pago por PaymentIntentId en el webhook de Stripe
        builder.HasIndex(p => p.StripePaymentIntentId).IsUnique();

        // Estado como entero en la BD
        builder.Property(p => p.Status).HasConversion<int>();
    }
}
