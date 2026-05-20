using MediatR;
using Intelectia.Application.Common.Exceptions;
using Intelectia.Application.Common.Interfaces;
using Intelectia.Domain.Entities;
using Intelectia.Domain.Enums;
using Intelectia.Domain.Interfaces;
using Intelectia.Domain.Interfaces.Repositories;
using Intelectia.Shared.DTOs.Commerce;

namespace Intelectia.Application.Features.Commerce.Commands.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, CreateOrderResponse>
{
    private readonly ICartRepository   _cartRepository;
    private readonly IOrderRepository  _orderRepository;
    private readonly IRepository<Payment> _paymentRepository;
    private readonly IPaymentService   _paymentService;
    private readonly IUnitOfWork       _unitOfWork;

    public CreateOrderCommandHandler(
        ICartRepository cartRepository,
        IOrderRepository orderRepository,
        IRepository<Payment> paymentRepository,
        IPaymentService paymentService,
        IUnitOfWork unitOfWork)
    {
        _cartRepository    = cartRepository;
        _orderRepository   = orderRepository;
        _paymentRepository = paymentRepository;
        _paymentService    = paymentService;
        _unitOfWork        = unitOfWork;
    }

    public async Task<CreateOrderResponse> Handle(
        CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Cargamos el carrito con todos sus items activos
        var cart = await _cartRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        var activeItems = cart?.Items.Where(i => !i.IsDeleted).ToList()
                          ?? new List<CartItem>();

        if (activeItems.Count == 0)
            throw new ValidationException([
                new FluentValidation.Results.ValidationFailure("Cart", "El carrito está vacío.")
            ]);

        // Calculamos el total desde los precios capturados en el carrito
        var total = activeItems.Sum(i => i.PriceSnapshot);

        // Creamos el pedido con los items
        var order = new Order
        {
            UserId = request.UserId,
            Total  = total,
            Status = OrderStatus.Pending
        };

        foreach (var cartItem in activeItems)
        {
            order.Items.Add(new OrderItem
            {
                BookId        = cartItem.BookId,
                PriceSnapshot = cartItem.PriceSnapshot
            });
        }

        await _orderRepository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Creamos el PaymentIntent en Stripe con el total en centavos
        var (paymentIntentId, clientSecret) = await _paymentService
            .CreatePaymentIntentAsync(order.Id, total, cancellationToken);

        // Guardamos los datos del pago
        var payment = new Payment
        {
            OrderId               = order.Id,
            StripePaymentIntentId = paymentIntentId,
            StripeClientSecret    = clientSecret,
            Status                = PaymentStatus.Pending,
            AmountInCents         = (long)(total * 100)
        };

        await _paymentRepository.AddAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateOrderResponse
        {
            OrderId      = order.Id,
            ClientSecret = clientSecret,
            Total        = total
        };
    }
}
