namespace Intelectia.Shared.DTOs.Marketplace;

public class BookSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public decimal Price { get; set; }
    public string Format { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
}
