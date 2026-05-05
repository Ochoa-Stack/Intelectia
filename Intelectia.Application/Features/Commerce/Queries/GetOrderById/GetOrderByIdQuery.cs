using MediatR;
using Intelectia.Shared.DTOs.Commerce;

namespace Intelectia.Application.Features.Commerce.Queries.GetOrderById;

public record GetOrderByIdQuery(Guid OrderId, Guid UserId) : IRequest<OrderDto>;
