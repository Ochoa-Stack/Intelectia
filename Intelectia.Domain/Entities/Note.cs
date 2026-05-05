using Intelectia.Domain.Common;

namespace Intelectia.Domain.Entities;

public class Note : BaseEntity
{
    // Usuario dueño de la nota
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    // Libro al que pertenece la nota; null si es una nota general sin libro asociado
    public Guid? BookId { get; set; }
    public Book? Book { get; set; }

    // Título corto de la nota
    public string Title { get; set; } = string.Empty;

    // Contenido completo de la nota
    public string Content { get; set; } = string.Empty;

    // Página del libro donde se tomó la nota
    public int? PageNumber { get; set; }

    // Texto resaltado del libro al que hace referencia esta nota
    public string? HighlightedText { get; set; }

    // Color del resaltado en formato hex (ej: '#FFC107')
    public string? HighlightColor { get; set; }
}
