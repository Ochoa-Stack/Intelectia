using DeepL;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Intelectia.Application.Common.Interfaces;

namespace Intelectia.Infrastructure.Services;

public class DeepLTranslationService : ITranslationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DeepLTranslationService> _logger;

    public DeepLTranslationService(
        IConfiguration configuration,
        ILogger<DeepLTranslationService> logger)
    {
        _configuration = configuration;
        _logger        = logger;
    }

    public async Task<string> TranslateAsync(
        string text,
        string targetLanguage,
        string? sourceLanguage = null,
        CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["ExternalServices:DeepL:ApiKey"]
            ?? throw new InvalidOperationException(
                "DeepL ApiKey no está configurada. " +
                "Ejecuta: dotnet user-secrets set \"ExternalServices:DeepL:ApiKey\" \"TU_CLAVE:fx\"");

        // Instanciamos el translator aquí para evitar capturar la ApiKey en el constructor
        var translator = new Translator(apiKey);

        try
        {
            // DeepL detecta el idioma origen automáticamente si sourceLanguage es null
            var result = await translator.TranslateTextAsync(
                text,
                sourceLanguage,
                targetLanguage,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Traducción completada: {Chars} caracteres ({From} → {To})",
                text.Length, sourceLanguage ?? "auto", targetLanguage);

            return result.Text;
        }
        catch (DeepLException ex)
        {
            _logger.LogError(ex, "Error al traducir con DeepL");
            throw new InvalidOperationException("No se pudo completar la traducción en este momento.", ex);
        }
    }
}
