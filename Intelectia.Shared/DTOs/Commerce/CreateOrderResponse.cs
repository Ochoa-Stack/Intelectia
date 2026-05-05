namespace Intelectia.Shared.DTOs.Commerce;

public class CreateOrderResponse
{
    // ID del pedido creado en la base de datos
    public Guid OrderId { get; set; }

    // Client secret que el cliente WPF usa para confirmar el pago con Stripe.js
    public string ClientSecret { get; set; } = string.Empty;

    // Total a cobrar; para mostrarlo al usuario antes de confirmar el pago
    public decimal Total { get; set; }
}
