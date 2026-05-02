using MediatR;
using Intelectia.Shared.DTOs.Commerce;

namespace Intelectia.Application.Features.Commerce.Commands.CreateOrder;

// Crea un pedido desde el carrito del usuario e inicia el PaymentIntent en Stripe
public record CreateOrderCommand(Guid UserId) : IRequest<CreateOrderResponse>;
