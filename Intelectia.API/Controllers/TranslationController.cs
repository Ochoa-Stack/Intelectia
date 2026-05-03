using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Intelectia.Application.Features.Library.Commands.TranslateText;
using Intelectia.Shared.DTOs.Library;

namespace Intelectia.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TranslationController : ControllerBase
{
    private readonly IMediator _mediator;

    public TranslationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Traduce un texto con DeepL y guarda el resultado en el historial del usuario
    [HttpPost("translate")]
    public async Task<IActionResult> Translate(
        [FromBody] TranslateRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(new TranslateTextCommand(
            userId,
            request.Text,
            request.TargetLanguage,
            request.SourceLanguage,
            request.BookId), cancellationToken);

        return Ok(result);
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Token inválido.");
        return Guid.Parse(claim);
    }
}
