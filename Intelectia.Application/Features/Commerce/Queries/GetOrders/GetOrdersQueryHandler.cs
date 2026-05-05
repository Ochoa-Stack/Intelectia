using AutoMapper;
using MediatR;
using Intelectia.Domain.Interfaces.Repositories;
using Intelectia.Shared.DTOs.Commerce;

namespace Intelectia.Application.Features.Commerce.Queries.GetOrders;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, IReadOnlyList<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetOrdersQueryHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper          = mapper;
    }

    public async Task<IReadOnlyList<OrderDto>> Handle(
        GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        return _mapper.Map<IReadOnlyList<OrderDto>>(orders);
    }
}
