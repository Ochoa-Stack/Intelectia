namespace Intelectia.Domain.Enums;

public enum OrderStatus
{
    // Pedido creado, esperando pago
    Pending = 1,

    // Pago confirmado por Stripe
    Paid = 2,

    // Pedido cancelado por el usuario o el sistema
    Cancelled = 3,

    // Reembolso procesado
    Refunded = 4
}
