namespace Intelectia.Application.Common.Interfaces;

public interface ITranslationService
{
    // Traduce el texto al idioma destino usando DeepL
    // sourceLanguage es opcional; DeepL detecta automáticamente si es null
    Task<string> TranslateAsync(
        string text,
        string targetLanguage,
        string? sourceLanguage = null,
        CancellationToken cancellationToken = default);
}
