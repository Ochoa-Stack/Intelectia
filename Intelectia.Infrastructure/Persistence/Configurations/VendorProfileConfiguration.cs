using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Intelectia.Domain.Entities;

namespace Intelectia.Infrastructure.Persistence.Configurations;

public class VendorProfileConfiguration : IEntityTypeConfiguration<VendorProfile>
{
    public void Configure(EntityTypeBuilder<VendorProfile> builder)
    {
        builder.ToTable("VendorProfiles");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.BusinessName).IsRequired().HasMaxLength(200);
        builder.Property(v => v.Description).HasMaxLength(1000);
        builder.Property(v => v.StripeAccountId).HasMaxLength(100);
    }
}
