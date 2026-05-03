using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Intelectia.Application.Features.Library.Commands.CreateNote;
using Intelectia.Application.Features.Library.Commands.DeleteNote;
using Intelectia.Application.Features.Library.Queries.GetNotes;
using Intelectia.Shared.DTOs.Library;

namespace Intelectia.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotesController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Devuelve las notas del usuario; bookId opcional para filtrar por libro
    [HttpGet]
    public async Task<IActionResult> GetNotes(
        [FromQuery] Guid? bookId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(new GetNotesQuery(userId, bookId), cancellationToken);
        return Ok(result);
    }

    // Crea una nota nueva y devuelve el DTO con el Id asignado
    [HttpPost]
    public async Task<IActionResult> CreateNote(
        [FromBody] CreateNoteRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(new CreateNoteCommand(
            userId,
            request.BookId,
            request.Title,
            request.Content,
            request.PageNumber,
            request.HighlightedText,
            request.HighlightColor), cancellationToken);

        return CreatedAtAction(nameof(GetNotes), result);
    }

    // Aplica soft delete a una nota por ID
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteNote(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await _mediator.Send(new DeleteNoteCommand(id, userId), cancellationToken);
        return NoContent();
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Token inválido.");
        return Guid.Parse(claim);
    }
}
