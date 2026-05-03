namespace Intelectia.Shared.DTOs.Library;

public class CreateNoteRequest
{
    public Guid? BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int? PageNumber { get; set; }
    public string? HighlightedText { get; set; }
    public string? HighlightColor { get; set; }
}
