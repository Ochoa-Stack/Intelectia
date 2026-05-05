using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Intelectia.Application.Features.Commerce.Commands.AddToCart;
using Intelectia.Application.Features.Commerce.Commands.RemoveFromCart;
using Intelectia.Application.Features.Commerce.Queries.GetCart;

namespace Intelectia.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly IMediator _mediator;

    public CartController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Devuelve el carrito del usuario autenticado. Lo crea vacío si no existe
    [HttpGet]
    public async Task<IActionResult> GetCart(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCartQuery(GetUserId()), cancellationToken);
        return Ok(result);
    }

    // Agrega un libro al carrito
    [HttpPost("items")]
    public async Task<IActionResult> AddItem(
        [FromBody] AddToCartRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new AddToCartCommand(GetUserId(), request.BookId), cancellationToken);
        return Ok(result);
    }

    // Elimina un item del carrito por su ID
    [HttpDelete("items/{cartItemId:guid}")]
    public async Task<IActionResult> RemoveItem(
        Guid cartItemId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new RemoveFromCartCommand(GetUserId(), cartItemId), cancellationToken);
        return Ok(result);
    }

    // Extrae el ID del usuario autenticado desde el JWT
    private Guid GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Token inválido.");
        return Guid.Parse(claim);
    }
}

// Request body para agregar un libro al carrito
public record AddToCartRequest(Guid BookId);
