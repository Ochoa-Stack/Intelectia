using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Intelectia.Application.Features.Commerce.Commands.CancelOrder;
using Intelectia.Application.Features.Commerce.Commands.CreateOrder;
using Intelectia.Application.Features.Commerce.Queries.GetOrderById;
using Intelectia.Application.Features.Commerce.Queries.GetOrders;

namespace Intelectia.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary> Devuelve el historial de pedidos del usuario autenticado </summary>
    [HttpGet]
    public async Task<IActionResult> GetOrders(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetOrdersQuery(GetUserId()), cancellationToken);
        return Ok(result);
    }

    /// <summary> Devuelve el detalle de un pedido. Solo el dueño puede verlo </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetOrderByIdQuery(id, GetUserId()), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Crea un pedido desde el carrito e inicia el PaymentIntent en Stripe
    /// Devuelve el ClientSecret para que el cliente confirme el pago
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateOrder(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateOrderCommand(GetUserId()), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.OrderId }, result);
    }

    /// <summary> Cancela un pedido pendiente de pago. Solo el dueño puede cancelarlo </summary>
    [HttpPut("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new CancelOrderCommand(id, GetUserId()), cancellationToken);
        return NoContent();
    }

    // Extrae el ID del usuario autenticado desde el JWT
    private Guid GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Token inválido.");
        return Guid.Parse(claim);
    }
}
