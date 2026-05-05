using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Intelectia.Application.Features.Groups.Commands.CreateGroup;
using Intelectia.Application.Features.Groups.Commands.JoinGroup;
using Intelectia.Application.Features.Groups.Commands.LeaveGroup;
using Intelectia.Application.Features.Groups.Queries.GetGroupMessages;
using Intelectia.Application.Features.Groups.Queries.GetMyGroups;
using Intelectia.Application.Features.Groups.Queries.GetPublicGroups;
using Intelectia.Shared.DTOs.Groups;

namespace Intelectia.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GroupsController : ControllerBase
{
    /* Definimos e inyectamos la instancia para la gestionar
     * el envío de comandos y consultas del controlador */
    private readonly IMediator _mediator;

    public GroupsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Devuelve los grupos del usuario autenticado
    [HttpGet("my")]
    public async Task<IActionResult> GetMyGroups(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(new GetMyGroupsQuery(userId), cancellationToken);
        return Ok(result);
    }

    // Devuelve grupos públicos para explorar
    [HttpGet("public")]
    public async Task<IActionResult> GetPublicGroups(
        [FromQuery] string? search, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(
            new GetPublicGroupsQuery(userId, search), cancellationToken);
        return Ok(result);
    }

    // Crea un grupo nuevo
    [HttpPost]
    public async Task<IActionResult> CreateGroup(
        [FromBody] CreateGroupRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(new CreateGroupCommand(
            userId, request.Name, request.Description, request.IsPublic), cancellationToken);
        return CreatedAtAction(nameof(GetMyGroups), result);
    }

    // Unirse a un grupo público
    [HttpPost("{id:guid}/join")]
    public async Task<IActionResult> JoinGroup(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await _mediator.Send(new JoinGroupCommand(id, userId), cancellationToken);
        return NoContent();
    }

    // Abandonar un grupo
    [HttpDelete("{id:guid}/leave")]
    public async Task<IActionResult> LeaveGroup(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await _mediator.Send(new LeaveGroupCommand(id, userId), cancellationToken);
        return NoContent();
    }

    // Trae el historial paginado de mensajes del grupo
    [HttpGet("{id:guid}/messages")]
    public async Task<IActionResult> GetMessages(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(
            new GetGroupMessagesQuery(id, userId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Token inválido.");
        return Guid.Parse(claim);
    }
}
