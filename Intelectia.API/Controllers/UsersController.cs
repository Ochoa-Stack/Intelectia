using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Intelectia.Application.Features.Profile.Commands.ChangePassword;
using Intelectia.Application.Features.Profile.Commands.UpdateProfile;
using Intelectia.Application.Features.Profile.Queries.GetProfile;
using Intelectia.Shared.DTOs.Profile;

namespace Intelectia.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Devuelve el perfil del usuario autenticado
    [HttpGet("me")]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(new GetProfileQuery(userId), cancellationToken);
        return Ok(result);
    }

    // Actualiza los datos del perfil
    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(new UpdateProfileCommand(
            userId, request.FirstName, request.LastName), cancellationToken);
        return Ok(result);
    }

    // Cambia la contraseña del usuario
    [HttpPut("me/password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await _mediator.Send(new ChangePasswordCommand(
            userId,
            request.CurrentPassword,
            request.NewPassword,
            request.ConfirmNewPassword), cancellationToken);

        return Ok(new { message = "Contraseña actualizada exitosamente." });
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Token inválido.");
        return Guid.Parse(claim);
    }
}
