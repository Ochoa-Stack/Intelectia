using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Intelectia.Application.Common.Exceptions;
using Intelectia.Application.Common.Interfaces;
using Intelectia.Domain.Entities;
using Intelectia.Domain.Interfaces;
using Intelectia.Domain.Interfaces.Repositories;
using Intelectia.Shared.DTOs.Commerce;

namespace Intelectia.Application.Features.Commerce.Commands.AddToCart;

public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, CartDto>
{
    private readonly ICartRepository _cartRepository;
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AddToCartCommandHandler(
        ICartRepository cartRepository,
        IApplicationDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _cartRepository = cartRepository;
        _context        = context;
        _unitOfWork     = unitOfWork;
        _mapper         = mapper;
    }

    public async Task<CartDto> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        // Verificamos que el libro exista y esté activo
        var book = await _context.Books
            .FirstOrDefaultAsync(b => b.Id == request.BookId && !b.IsDeleted, cancellationToken);

        if (book is null)
            throw new NotFoundException(nameof(Book), request.BookId);

        // Verificamos que el usuario no tenga el libro ya en su biblioteca
        var alreadyOwned = await _context.UserBooks
            .AnyAsync(ub => ub.UserId == request.UserId && ub.BookId == request.BookId,
                cancellationToken);

        if (alreadyOwned)
            throw new ConflictException("Ya tienes este libro en tu biblioteca.");

        // Buscamos o creamos el carrito del usuario
        var cart = await _cartRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (cart is null)
        {
            cart = new Cart { UserId = request.UserId };
            await _cartRepository.AddAsync(cart, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            cart = await _cartRepository.GetByUserIdAsync(request.UserId, cancellationToken)
                ?? throw new InvalidOperationException("No se pudo crear el carrito.");
        }

        // Verificamos que el libro no esté ya en el carrito
        if (cart.Items.Any(i => i.BookId == request.BookId && !i.IsDeleted))
            throw new ConflictException("Este libro ya está en tu carrito.");

        // Agregamos el item capturando el precio actual del libro
        var cartItem = new CartItem
        {
            CartId        = cart.Id,
            BookId        = request.BookId,
            PriceSnapshot = book.Price
        };

        await _context.CartItems.AddAsync(cartItem, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Recargamos el carrito completo para devolver el estado actualizado
        var updatedCart = await _cartRepository.GetByUserIdAsync(request.UserId, cancellationToken)
            ?? throw new InvalidOperationException("Error al cargar el carrito.");

        return _mapper.Map<CartDto>(updatedCart);
    }
}
