using Intelectia.Domain.Common;

namespace Intelectia.Domain.Entities;

public class Review : BaseEntity
{
    // Libro al que pertenece la reseña
    public Guid BookId { get; set; }
    public Book Book { get; set; } = null!;

    // Usuario que escribió la reseña
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    // Calificación del 1 al 5
    public int Rating { get; set; }

    // Texto opcional de la reseña
    public string? Comment { get; set; }
}
