namespace Intelectia.Shared.DTOs.Library;

public class CitationDto
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public string GeneratedText { get; set; } = string.Empty;
    public int? PageNumber { get; set; }
    public DateTime CreatedAt { get; set; }
}
