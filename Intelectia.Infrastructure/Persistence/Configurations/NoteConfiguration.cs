using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Intelectia.Domain.Entities;

namespace Intelectia.Infrastructure.Persistence.Configurations;

public class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.ToTable("Notes");
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Title).IsRequired().HasMaxLength(200);
        builder.Property(n => n.Content).IsRequired().HasMaxLength(10000);
        builder.Property(n => n.HighlightedText).HasMaxLength(2000);
        builder.Property(n => n.HighlightColor).HasMaxLength(10);

        // Índice compuesto para cargar rápido notas de un usuario por libro
        builder.HasIndex(n => new { n.UserId, n.BookId });

        builder.HasOne(n => n.User)
               .WithMany(u => u.Notes)
               .HasForeignKey(n => n.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        // BookId nullable; si el libro se elimina, la nota queda huérfana (no se borra)
        builder.HasOne(n => n.Book)
               .WithMany()
               .HasForeignKey(n => n.BookId)
               .OnDelete(DeleteBehavior.SetNull)
               .IsRequired(false);
    }
}
