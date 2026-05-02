using MediatR;
using Intelectia.Shared.DTOs.Commerce;

namespace Intelectia.Application.Features.Commerce.Queries.GetCart;

public record GetCartQuery(Guid UserId) : IRequest<CartDto>;
