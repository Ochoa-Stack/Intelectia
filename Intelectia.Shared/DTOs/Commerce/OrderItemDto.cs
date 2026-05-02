namespace Intelectia.Shared.DTOs.Commerce;

public class OrderItemDto
{
    public Guid BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string BookAuthor { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public decimal PriceSnapshot { get; set; }
}
