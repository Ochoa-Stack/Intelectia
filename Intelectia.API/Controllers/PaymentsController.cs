using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Intelectia.Application.Features.Commerce.Commands.ConfirmPayment;

namespace Intelectia.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IMediator mediator,
        IConfiguration configuration,
        ILogger<PaymentsController> logger)
    {
        _mediator      = mediator;
        _configuration = configuration;
        _logger        = logger;
    }

    /* Endpoint que Stripe llama cuando ocurre un evento de pago.
     * No requiere JWT; la autenticidad se verifica con la firma de Stripe */
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook(CancellationToken cancellationToken)
    {
        // Leemos el body crud; Stripe verifica la firma sobre el payload exacto
        var json = await new StreamReader(HttpContext.Request.Body)
            .ReadToEndAsync(cancellationToken);

        var webhookSecret = _configuration["ExternalServices:Stripe:WebhookSecret"]
            ?? throw new InvalidOperationException("Stripe WebhookSecret no configurado.");

        try
        {
            // Verificamos la firma; rechaza cualquier webhook que no venga de Stripe
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                webhookSecret);

            _logger.LogInformation(
                "Webhook recibido: {EventType} / {EventId}", stripeEvent.Type, stripeEvent.Id);

            // Solo procesamos el evento de pago confirmado
            if (stripeEvent.Type == EventTypes.PaymentIntentSucceeded)
            {
                var paymentIntent = (PaymentIntent)stripeEvent.Data.Object;
                await _mediator.Send(
                    new ConfirmPaymentCommand(paymentIntent.Id), cancellationToken);
            }

            // Devolvemos 200 para que Stripe sepa que recibimos el evento
            return Ok();
        }
        catch (StripeException ex)
        {
            // Firma inválida; rechazamos con 400 sin exponer detalles
            _logger.LogWarning("Webhook con firma inválida: {Message}", ex.Message);
            return BadRequest();
        }
    }
}
