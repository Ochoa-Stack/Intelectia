using Intelectia.Domain.Common;

namespace Intelectia.Domain.Entities;

public class Category : BaseEntity
{
    // Nombre visible de la categoría
    public string Name { get; set; } = string.Empty;

    // Slug para URLs amigables (ej: "ciencias-exactas")
    public string Slug { get; set; } = string.Empty;

    // Descripción opcional de la categoría
    public string? Description { get; set; }

    // Libros que pertenecen a esta categoría
    public ICollection<Book> Books { get; set; } = new List<Book>();
}
