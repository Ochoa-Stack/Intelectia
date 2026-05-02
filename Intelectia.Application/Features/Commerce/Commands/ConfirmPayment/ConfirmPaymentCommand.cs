using MediatR;

namespace Intelectia.Application.Features.Commerce.Commands.ConfirmPayment;

// Recibe el PaymentIntentId que Stripe confirmó; lo dispara el webhook
public record ConfirmPaymentCommand(string PaymentIntentId) : IRequest;
