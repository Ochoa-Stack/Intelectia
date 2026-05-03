using Intelectia.Domain.Common;
using Intelectia.Domain.Enums;

namespace Intelectia.Domain.Entities;

public class Citation : BaseEntity
{
    // Usuario que generó la cita
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    // Libro del que se genera la cita
    public Guid BookId { get; set; }
    public Book Book { get; set; } = null!;

    // Formato bibliográfico usado para generar la cita
    public CitationFormat Format { get; set; } = CitationFormat.APA;

    // Texto de la cita generado en el formato seleccionado
    public string GeneratedText { get; set; } = string.Empty;

    // Página específica citada; null si es una cita del libro completo
    public int? PageNumber { get; set; }
}
