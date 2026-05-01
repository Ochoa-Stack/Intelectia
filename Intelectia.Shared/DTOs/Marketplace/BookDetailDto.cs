namespace Intelectia.Shared.DTOs.Marketplace;

public class BookDetailDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public int PublishedYear { get; set; }
    public int PageCount { get; set; }
    public string Language { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Format { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public List<ReviewDto> Reviews { get; set; } = new();
}
