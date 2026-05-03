using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Intelectia.Application.Features.Library.Queries.GetUserBooks;

namespace Intelectia.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LibraryController : ControllerBase
{
    private readonly IMediator _mediator;

    public LibraryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Devuelve los libros adquiridos por el usuario autenticado
    [HttpGet("books")]
    public async Task<IActionResult> GetBooks(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(new GetUserBooksQuery(userId), cancellationToken);
        return Ok(result);
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Token inválido.");
        return Guid.Parse(claim);
    }
}
