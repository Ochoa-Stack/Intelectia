using Intelectia.Domain.Common;

namespace Intelectia.Domain.Entities;

public class TranslationHistory : BaseEntity
{
    // Usuario que realizó la traducción
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    // Texto original enviado a traducir
    public string SourceText { get; set; } = string.Empty;

    // Texto traducido recibido de DeepL
    public string TranslatedText { get; set; } = string.Empty;

    // Idioma de origen; código ISO 639-1, ej: "en"
    public string SourceLanguage { get; set; } = string.Empty;

    // Idioma de destino; código ISO 639-1, ej: "es"
    public string TargetLanguage { get; set; } = string.Empty;

    // Libro desde donde se inició la traducción; null si fue una traducción manual
    public Guid? BookId { get; set; }
    public Book? Book { get; set; }
}
