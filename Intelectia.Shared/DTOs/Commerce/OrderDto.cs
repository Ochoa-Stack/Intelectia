namespace Intelectia.Shared.DTOs.Commerce;

public class OrderDto
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();

    // ID del PaymentIntent de Stripe — se muestra en el recibo
    public string? StripePaymentIntentId { get; set; }
}
