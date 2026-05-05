using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Intelectia.Domain.Entities;

namespace Intelectia.Infrastructure.Persistence.Configurations;

public class CitationConfiguration : IEntityTypeConfiguration<Citation>
{
    public void Configure(EntityTypeBuilder<Citation> builder)
    {
        builder.ToTable("Citations");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.GeneratedText).IsRequired().HasMaxLength(3000);

        // Persistimos el enum como int; más eficiente que string para un campo de 4 valores
        builder.Property(c => c.Format).HasConversion<int>();

        builder.HasIndex(c => new { c.UserId, c.BookId });

        builder.HasOne(c => c.User)
               .WithMany(u => u.Citations)
               .HasForeignKey(c => c.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        // Restrict en Book; no se puede eliminar un libro con citas activas
        builder.HasOne(c => c.Book)
               .WithMany()
               .HasForeignKey(c => c.BookId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
