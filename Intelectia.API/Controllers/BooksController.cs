using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Intelectia.Application.Features.Marketplace.Commands.AddReview;
using Intelectia.Application.Features.Marketplace.Queries.GetBookById;
using Intelectia.Application.Features.Marketplace.Queries.GetBooks;
using Intelectia.Domain.Enums;
using Intelectia.Shared.DTOs.Marketplace;

namespace Intelectia.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IMediator _mediator;

    public BooksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary> Devuelve el catálogo paginado con filtros opcionales. Acceso público. </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetBooks(
        [FromQuery] int         page       = 1,
        [FromQuery] int         pageSize   = 12,
        [FromQuery] string?     search     = null,
        [FromQuery] Guid?       categoryId = null,
        [FromQuery] BookFormat? format     = null,
        [FromQuery] decimal?    minPrice   = null,
        [FromQuery] decimal?    maxPrice   = null,
        [FromQuery] string?     sortBy     = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetBooksQuery(page, pageSize, search, categoryId, format, minPrice, maxPrice, sortBy),
            cancellationToken);

        return Ok(result);
    }

    /// <summary> Devuelve el detalle de un libro con sus reseñas. Acceso público. </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetBookByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Agrega una reseña a un libro. Requiere autenticación y haber adquirido el libro.
    /// </summary>
    [HttpPost("{id:guid}/reviews")]
    [Authorize]
    public async Task<IActionResult> AddReview(
        Guid id,
        [FromBody] AddReviewRequest request,
        CancellationToken cancellationToken)
    {
        // Leemos el ID del usuario autenticado desde el claim del JWT
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Token inválido.");

        var userId = Guid.Parse(userIdClaim);

        var result = await _mediator.Send(
            new AddReviewCommand(id, userId, request.Rating, request.Comment),
            cancellationToken);

        // 201 Created apuntando al detalle del libro donde aparecerá la reseña
        return CreatedAtAction(nameof(GetById), new { id }, result);
    }
}
