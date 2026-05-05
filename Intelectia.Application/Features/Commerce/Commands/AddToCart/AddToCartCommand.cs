using MediatR;
using Intelectia.Shared.DTOs.Commerce;

namespace Intelectia.Application.Features.Commerce.Commands.AddToCart;

public record AddToCartCommand(Guid UserId, Guid BookId) : IRequest<CartDto>;
