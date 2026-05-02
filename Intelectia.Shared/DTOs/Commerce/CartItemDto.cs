namespace Intelectia.Shared.DTOs.Commerce;

public class CartItemDto
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string BookAuthor { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public string Format { get; set; } = string.Empty;

    // Precio capturado al momento de agregar al carrito
    public decimal PriceSnapshot { get; set; }
}
