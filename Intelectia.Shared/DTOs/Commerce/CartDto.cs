namespace Intelectia.Shared.DTOs.Commerce;

public class CartDto
{
    public Guid Id { get; set; }
    public List<CartItemDto> Items { get; set; } = new();

    // Total calculado desde los PriceSnapshots de los items
    public decimal Total => Items.Sum(i => i.PriceSnapshot);
    public int ItemCount => Items.Count;
}
