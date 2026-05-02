using MediatR;
using Intelectia.Shared.DTOs.Commerce;

namespace Intelectia.Application.Features.Commerce.Queries.GetOrders;

public record GetOrdersQuery(Guid UserId) : IRequest<IReadOnlyList<OrderDto>>;
