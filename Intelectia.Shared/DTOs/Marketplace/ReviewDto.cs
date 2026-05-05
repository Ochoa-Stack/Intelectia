namespace Intelectia.Shared.DTOs.Marketplace;

public class ReviewDto
{
    public Guid Id { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}
