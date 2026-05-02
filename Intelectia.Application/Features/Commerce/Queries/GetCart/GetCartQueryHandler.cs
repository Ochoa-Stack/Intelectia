using AutoMapper;
using MediatR;
using Intelectia.Domain.Entities;
using Intelectia.Domain.Interfaces;
using Intelectia.Domain.Interfaces.Repositories;
using Intelectia.Shared.DTOs.Commerce;

namespace Intelectia.Application.Features.Commerce.Queries.GetCart;

public class GetCartQueryHandler : IRequestHandler<GetCartQuery, CartDto>
{
    private readonly ICartRepository _cartRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCartQueryHandler(
        ICartRepository cartRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _cartRepository = cartRepository;
        _unitOfWork     = unitOfWork;
        _mapper         = mapper;
    }

    public async Task<CartDto> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        // Buscamos el carrito del usuario; si no existe lo creamos vacío
        var cart = await _cartRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        if (cart is null)
        {
            cart = new Cart { UserId = request.UserId };
            await _cartRepository.AddAsync(cart, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Recargamos con las relaciones para el mapeo
            cart = await _cartRepository.GetByUserIdAsync(request.UserId, cancellationToken)
                ?? cart;
        }

        return _mapper.Map<CartDto>(cart);
    }
}
