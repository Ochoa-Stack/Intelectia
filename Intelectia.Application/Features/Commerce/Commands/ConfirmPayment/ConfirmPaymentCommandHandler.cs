using MediatR;
using Microsoft.Extensions.Logging;
using Intelectia.Domain.Entities;
using Intelectia.Domain.Enums;
using Intelectia.Domain.Interfaces;
using Intelectia.Domain.Interfaces.Repositories;

namespace Intelectia.Application.Features.Commerce.Commands.ConfirmPayment;

public class ConfirmPaymentCommandHandler : IRequestHandler<ConfirmPaymentCommand>
{
    private readonly IOrderRepository      _orderRepository;
    private readonly IRepository<UserBook> _userBookRepository;
    private readonly IUnitOfWork           _unitOfWork;
    private readonly ILogger<ConfirmPaymentCommandHandler> _logger;

    public ConfirmPaymentCommandHandler(
        IOrderRepository orderRepository,
        IRepository<UserBook> userBookRepository,
        IUnitOfWork unitOfWork,
        ILogger<ConfirmPaymentCommandHandler> logger)
    {
        _orderRepository    = orderRepository;
        _userBookRepository = userBookRepository;
        _unitOfWork         = unitOfWork;
        _logger             = logger;
    }

    public async Task Handle(ConfirmPaymentCommand request, CancellationToken cancellationToken)
    {
        // Buscamos el pedido por el PaymentIntentId que Stripe nos envió en el webhook
        var order = await _orderRepository.GetByPaymentIntentIdAsync(
            request.PaymentIntentId, cancellationToken);

        if (order is null)
        {
            _logger.LogWarning(
                "Webhook recibido para PaymentIntent desconocido: {Id}",
                request.PaymentIntentId);
            return;
        }

        // Stripe puede reenviar el mismo webhook más de una vez (Idempotencia)
        if (order.Status == OrderStatus.Paid)
        {
            _logger.LogInformation(
                "Pedido {OrderId} ya estaba pagado — webhook duplicado ignorado.", order.Id);
            return;
        }

        // Actualizamos el estado del pedido
        order.Status = OrderStatus.Paid;
        order.PaidAt = DateTime.UtcNow;

        // Actualizamos el estado del pago
        if (order.Payment is not null)
        {
            order.Payment.Status      = PaymentStatus.Succeeded;
            order.Payment.ConfirmedAt = DateTime.UtcNow;
        }

        _orderRepository.Update(order);

        // Entregamos los libros digitales a la biblioteca del usuario
        var digitalItemsAdded = 0;
        foreach (var item in order.Items)
        {
            // Los libros físicos se entregan por correo; no van a la biblioteca digital
            if (item.Book.Format == BookFormat.Physical)
                continue;

            // No duplicar si el libro ya está en la biblioteca (Idempotencia)
            var alreadyOwned = await _userBookRepository
                .AnyAsync(ub => ub.UserId == order.UserId && ub.BookId == item.BookId,
                    cancellationToken);

            if (alreadyOwned)
                continue;

            await _userBookRepository.AddAsync(new UserBook
            {
                UserId     = order.UserId,
                BookId     = item.BookId,
                AcquiredAt = DateTime.UtcNow
            }, cancellationToken);

            digitalItemsAdded++;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Pedido {OrderId} confirmado. {Digital} libro(s) digital(es) entregados.",
            order.Id, digitalItemsAdded);
    }
}
