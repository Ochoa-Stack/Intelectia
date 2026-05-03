namespace Intelectia.Shared.DTOs.Library;

public class UserBookDto
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public string Format { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public DateTime AcquiredAt { get; set; }
    public int LastPageRead { get; set; }
    public double ReadingProgress { get; set; }
}
