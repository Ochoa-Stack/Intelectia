using AutoMapper;
using MediatR;
using Intelectia.Application.Common.Exceptions;
using Intelectia.Domain.Entities;
using Intelectia.Domain.Interfaces.Repositories;
using Intelectia.Shared.DTOs.Commerce;

namespace Intelectia.Application.Features.Commerce.Queries.GetOrderById;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetOrderByIdQueryHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper          = mapper;
    }

    public async Task<OrderDto> Handle(
        GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(
            request.OrderId, cancellationToken);

        // Verificamos que el pedido exista y pertenezca al usuario que lo solicita
        if (order is null || order.UserId != request.UserId)
            throw new NotFoundException(nameof(Order), request.OrderId);

        return _mapper.Map<OrderDto>(order);
    }
}
