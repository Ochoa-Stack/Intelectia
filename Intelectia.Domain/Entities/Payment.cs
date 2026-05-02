using Intelectia.Domain.Common;
using Intelectia.Domain.Enums;

namespace Intelectia.Domain.Entities;

public class Payment : BaseEntity
{
    // Pedido al que corresponde este pago; relación 1:1
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    // ID del PaymentIntent generado por Stripe
    public string StripePaymentIntentId { get; set; } = string.Empty;

    // Client secret que el cliente usa para confirmar el pago con Stripe.js
    public string StripeClientSecret { get; set; } = string.Empty;

    // Estado actual del pago en Stripe
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    // Monto cobrado en centavos; como lo maneja Stripe internamente
    public long AmountInCents { get; set; }

    // Fecha en que el pago fue confirmado por Stripe
    public DateTime? ConfirmedAt { get; set; }
}
