using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Intelectia.Domain.Entities;

namespace Intelectia.Infrastructure.Persistence.Configurations;

public class TranslationHistoryConfiguration : IEntityTypeConfiguration<TranslationHistory>
{
    public void Configure(EntityTypeBuilder<TranslationHistory> builder)
    {
        builder.ToTable("TranslationHistories");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.SourceText).IsRequired().HasMaxLength(5000);
        builder.Property(t => t.TranslatedText).IsRequired().HasMaxLength(5000);

        // ISO 639-1 codes -> 2 chars; reservamos 10 para codes compuestos como 'ZH-HANS'
        builder.Property(t => t.SourceLanguage).IsRequired().HasMaxLength(10);
        builder.Property(t => t.TargetLanguage).IsRequired().HasMaxLength(10);

        // Índice simple en UserId para cargar el historial del usuario
        builder.HasIndex(t => t.UserId);

        builder.HasOne(t => t.User)
               .WithMany(u => u.TranslationHistories)
               .HasForeignKey(t => t.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        // BookId nullable; null si fue una traducción manual sin contexto de libro
        builder.HasOne(t => t.Book)
               .WithMany()
               .HasForeignKey(t => t.BookId)
               .OnDelete(DeleteBehavior.SetNull)
               .IsRequired(false);
    }
}
