using MediatR;

namespace Intelectia.Application.Features.Commerce.Commands.CancelOrder;

public record CancelOrderCommand(Guid OrderId, Guid UserId) : IRequest;
