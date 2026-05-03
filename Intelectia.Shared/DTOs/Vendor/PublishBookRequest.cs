namespace Intelectia.Shared.DTOs.Vendor;

public class PublishBookRequest
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public int PublishedYear { get; set; }
    public int PageCount { get; set; }
    public string Language { get; set; } = "es";
    public decimal Price { get; set; }
    public string Format { get; set; } = "PDF";
    public Guid CategoryId { get; set; }
}
