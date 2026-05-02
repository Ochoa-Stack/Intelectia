using MediatR;
using Intelectia.Application.Common.Exceptions;
using Intelectia.Domain.Entities;
using Intelectia.Domain.Enums;
using Intelectia.Domain.Interfaces;
using Intelectia.Domain.Interfaces.Repositories;

namespace Intelectia.Application.Features.Commerce.Commands.CancelOrder;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelOrderCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork      = unitOfWork;
    }

    public async Task Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(
            request.OrderId, cancellationToken);

        // Solo el dueño del pedido puede cancelarlo
        if (order is null || order.UserId != request.UserId)
            throw new NotFoundException(nameof(Order), request.OrderId);

        // Solo se pueden cancelar pedidos que aún no fueron pagados
        if (order.Status != OrderStatus.Pending)
            throw new ConflictException("Solo se pueden cancelar pedidos pendientes de pago.");

        order.Status      = OrderStatus.Cancelled;
        order.CancelledAt = DateTime.UtcNow;

        _orderRepository.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
