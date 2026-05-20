using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Intelectia.Domain.Entities;

namespace Intelectia.Infrastructure.Persistence.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Books");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Title).IsRequired().HasMaxLength(300);
        builder.Property(b => b.Author).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Description).IsRequired().HasMaxLength(3000);
        builder.Property(b => b.ISBN).HasMaxLength(20);
        builder.Property(b => b.CoverImageUrl).HasMaxLength(1000);
        builder.Property(b => b.FileUrl).HasMaxLength(1000);
        builder.Property(b => b.Language).HasMaxLength(10);

        // Guardamos los enums como enteros en la base de datos
        builder.Property(b => b.Status).HasConversion<int>();
        builder.Property(b => b.Format).HasConversion<int>();

        // El precio con precisión monetaria estándar
        builder.Property(b => b.Price).HasColumnType("numeric(18,2)");

        // Índices para acelerar la búsqueda por título y autor
        builder.HasIndex(b => b.Title);
        builder.HasIndex(b => b.Author);

        // Un libro pertenece a una categoría; Restrict para no borrar libros al eliminar categorías
        builder.HasOne(b => b.Category)
               .WithMany(c => c.Books)
               .HasForeignKey(b => b.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);

        // Un libro pertenece a un vendedor; Restrict para proteger historial
        builder.HasOne(b => b.VendorProfile)
               .WithMany(v => v.Books)
               .HasForeignKey(b => b.VendorProfileId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
