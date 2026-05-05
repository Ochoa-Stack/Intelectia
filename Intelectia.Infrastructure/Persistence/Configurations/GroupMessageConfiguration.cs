using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Intelectia.Domain.Entities;

namespace Intelectia.Infrastructure.Persistence.Configurations;

public class GroupMessageConfiguration : IEntityTypeConfiguration<GroupMessage>
{
    public void Configure(EntityTypeBuilder<GroupMessage> builder)
    {
        builder.ToTable("GroupMessages");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Content).IsRequired().HasMaxLength(4000);

        // Índice para paginar mensajes de un grupo ordenados por fecha
        builder.HasIndex(m => new { m.GroupId, m.CreatedAt });

        builder.HasOne(m => m.User)
               .WithMany(u => u.GroupMessages)
               .HasForeignKey(m => m.UserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
