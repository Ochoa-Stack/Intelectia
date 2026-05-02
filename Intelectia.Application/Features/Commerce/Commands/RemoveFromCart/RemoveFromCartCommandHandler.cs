using AutoMapper;
using MediatR;
using Intelectia.Application.Common.Exceptions;
using Intelectia.Domain.Entities;
using Intelectia.Domain.Interfaces;
using Intelectia.Domain.Interfaces.Repositories;
using Intelectia.Shared.DTOs.Commerce;

namespace Intelectia.Application.Features.Commerce.Commands.RemoveFromCart;

public class RemoveFromCartCommandHandler : IRequestHandler<RemoveFromCartCommand, CartDto>
{
    private readonly ICartRepository _cartRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RemoveFromCartCommandHandler(
        ICartRepository cartRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _cartRepository = cartRepository;
        _unitOfWork     = unitOfWork;
        _mapper         = mapper;
    }

    public async Task<CartDto> Handle(RemoveFromCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        if (cart is null)
            throw new NotFoundException(nameof(Cart), request.UserId);

        // Buscamos el item dentro del carrito del usuario
        var item = cart.Items.FirstOrDefault(i => i.Id == request.CartItemId && !i.IsDeleted);

        if (item is null)
            throw new NotFoundException(nameof(CartItem), request.CartItemId);

        // Soft delete del item
        item.IsDeleted = true;
        item.DeletedAt = DateTime.UtcNow;

        _cartRepository.Update(cart);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Recargamos para devolver el carrito actualizado sin el item eliminado
        var updatedCart = await _cartRepository.GetByUserIdAsync(request.UserId, cancellationToken)
            ?? cart;

        return _mapper.Map<CartDto>(updatedCart);
    }
}
