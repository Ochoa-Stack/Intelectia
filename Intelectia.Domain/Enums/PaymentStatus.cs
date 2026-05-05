namespace Intelectia.Domain.Enums;

public enum PaymentStatus
{
    // Intento de pago iniciado en Stripe
    Pending = 1,

    // Pago confirmado exitosamente
    Succeeded = 2,

    // Pago rechazado por el procesador
    Failed = 3,

    // Pago reembolsado
    Refunded = 4
}
