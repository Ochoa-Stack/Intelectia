using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using Intelectia.Application.Common.Interfaces;

namespace Intelectia.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(IConfiguration configuration, ILogger<PaymentService> logger)
    {
        _logger = logger;

        // Configuramos la clave secreta de Stripe al instanciar el servicio
        StripeConfiguration.ApiKey = configuration["ExternalServices:Stripe:SecretKey"]
            ?? throw new InvalidOperationException(
                "Stripe SecretKey no está configurado. " +
                "Ejecuta: dotnet user-secrets set \"ExternalServices:Stripe:SecretKey\" \"sk_test_...\"");
    }

    public async Task<(string PaymentIntentId, string ClientSecret)> CreatePaymentIntentAsync(
        Guid orderId,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        // Stripe trabaja en centavos; por ende, multiplicamos por 100
        var amountInCents = (long)(amount * 100);

        var options = new PaymentIntentCreateOptions
        {
            Amount   = amountInCents,
            Currency = "usd",

            // El orderId en metadatos permite relacionar el webhook con el pedido interno
            Metadata = new Dictionary<string, string>
            {
                { "orderId", orderId.ToString() }
            },

            // Stripe determina automáticamente los métodos de pago disponibles
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true
            }
        };

        var service = new PaymentIntentService();
        var intent  = await service.CreateAsync(options, cancellationToken: cancellationToken);

        _logger.LogInformation(
            "PaymentIntent creado: {IntentId} para pedido {OrderId} por ${Amount}",
            intent.Id, orderId, amount);

        return (intent.Id, intent.ClientSecret);
    }

    public async Task RefundAsync(
        string paymentIntentId,
        CancellationToken cancellationToken = default)
    {
        var options = new RefundCreateOptions
        {
            PaymentIntent = paymentIntentId
        };

        var service = new RefundService();
        var refund  = await service.CreateAsync(options, cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Reembolso procesado: {RefundId} para PaymentIntent {IntentId}",
            refund.Id, paymentIntentId);
    }
}
