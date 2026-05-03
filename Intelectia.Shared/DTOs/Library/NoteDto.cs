namespace Intelectia.Shared.DTOs.Library;

public class NoteDto
{
    public Guid Id { get; set; }
    public Guid? BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int? PageNumber { get; set; }
    public string? HighlightedText { get; set; }
    public string? HighlightColor { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
