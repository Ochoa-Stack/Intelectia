using MediatR;
using Intelectia.Shared.DTOs.Commerce;

namespace Intelectia.Application.Features.Commerce.Commands.RemoveFromCart;

public record RemoveFromCartCommand(Guid UserId, Guid CartItemId) : IRequest<CartDto>;
