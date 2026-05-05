using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Intelectia.Domain.Entities;

namespace Intelectia.Infrastructure.Persistence.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");
        builder.HasKey(r => r.Id);

        // Un usuario solo puede reseñar el mismo libro una vez
        builder.HasIndex(r => new { r.UserId, r.BookId }).IsUnique();

        builder.Property(r => r.Rating).IsRequired();
        builder.Property(r => r.Comment).HasMaxLength(2000);

        // Cascade en Book; si se borra el libro se borran sus reseñas
        builder.HasOne(r => r.Book)
               .WithMany(b => b.Reviews)
               .HasForeignKey(r => r.BookId)
               .OnDelete(DeleteBehavior.Cascade);

        // Restrict en User; no borrar reseñas si se desactiva un usuario
        builder.HasOne(r => r.User)
               .WithMany(u => u.Reviews)
               .HasForeignKey(r => r.UserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
