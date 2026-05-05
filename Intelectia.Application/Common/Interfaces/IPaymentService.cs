namespace Intelectia.Application.Common.Interfaces;

public interface IPaymentService
{
    // Crea un PaymentIntent en Stripe y devuelve su ID y client secret
    // El client secret se pasa al cliente para confirmar el pago con Stripe.js
    Task<(string PaymentIntentId, string ClientSecret)> CreatePaymentIntentAsync(
        Guid orderId,
        decimal amount,
        CancellationToken cancellationToken = default);

    // Procesa el reembolso completo de un PaymentIntent ya cobrado
    Task RefundAsync(string paymentIntentId, CancellationToken cancellationToken = default);
}
