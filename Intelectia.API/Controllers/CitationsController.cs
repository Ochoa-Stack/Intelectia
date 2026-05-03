using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Intelectia.Application.Features.Library.Commands.GenerateCitation;
using Intelectia.Domain.Enums;
using Intelectia.Shared.DTOs.Library;

namespace Intelectia.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CitationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CitationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Genera y persiste una cita bibliográfica en el formato solicitado
    [HttpPost]
    public async Task<IActionResult> GenerateCitation(
        [FromBody] GenerateCitationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        // Parseamos el string del formato al enum; APA por defecto si no reconocemos el valor
        if (!Enum.TryParse<CitationFormat>(request.Format, ignoreCase: true, out var format))
            format = CitationFormat.APA;

        var result = await _mediator.Send(new GenerateCitationCommand(
            userId,
            request.BookId,
            format,
            request.PageNumber), cancellationToken);

        return Ok(result);
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Token inválido.");
        return Guid.Parse(claim);
    }
}
