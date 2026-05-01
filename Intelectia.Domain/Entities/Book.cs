using Intelectia.Domain.Common;
using Intelectia.Domain.Enums;

namespace Intelectia.Domain.Entities;

public class Book : BaseEntity
{
    // Datos básicos del libro
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;

    // URL de la portada almacenada en Azure Blob
    public string? CoverImageUrl { get; set; }

    // URL del archivo del libro en Azure Blob
    public string? FileUrl { get; set; }

    // Año de publicación original
    public int PublishedYear { get; set; }

    // Número de páginas
    public int PageCount { get; set; }

    // Idioma del contenido (ej: "es", "en")
    public string Language { get; set; } = "es";

    // Estado actual del libro en el catálogo
    public BookStatus Status { get; set; } = BookStatus.Draft;

    // Formato disponible para este libro
    public BookFormat Format { get; set; } = BookFormat.PDF;

    // Precio en la moneda base del sistema
    public decimal Price { get; set; }

    // Categoría a la que pertenece
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    // Vendedor que publicó el libro
    public Guid VendorProfileId { get; set; }
    public VendorProfile VendorProfile { get; set; } = null!;

    // Reseñas de usuarios que compraron este libro
    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    // Usuarios que tienen este libro en su biblioteca
    public ICollection<UserBook> UserBooks { get; set; } = new List<UserBook>();

    // Promedio calculado de calificaciones — se actualiza al agregar reseñas
    public double AverageRating { get; set; } = 0;

    // Total de reseñas para mostrar junto al promedio
    public int ReviewCount { get; set; } = 0;
}
