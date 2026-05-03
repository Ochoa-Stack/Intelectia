namespace Intelectia.Shared.DTOs.Library;

public class TranslateRequest
{
    public string Text { get; set; } = string.Empty;
    public string TargetLanguage { get; set; } = "ES";
    public string? SourceLanguage { get; set; }
    public Guid? BookId { get; set; }
}
