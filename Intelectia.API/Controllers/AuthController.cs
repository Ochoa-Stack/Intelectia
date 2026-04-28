using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Intelectia.Application.Features.Auth.Commands.ForgotPassword;
using Intelectia.Application.Features.Auth.Commands.Login;
using Intelectia.Application.Features.Auth.Commands.Logout;
using Intelectia.Application.Features.Auth.Commands.RefreshToken;
using Intelectia.Application.Features.Auth.Commands.Register;
using Intelectia.Application.Features.Auth.Commands.ResetPassword;
using Intelectia.Shared.DTOs.Auth;

namespace Intelectia.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Procesamos 'Registro' con email y contraseña
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RegisterCommand(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password,
            request.ConfirmPassword), cancellationToken);

        return CreatedAtAction(nameof(Register), result);
    }

    // Procesamos 'Login' con email y contraseña
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new LoginCommand(request.Email, request.Password), cancellationToken);

        return Ok(result);
    }

    // Renovamos el JWT usando el refresh token
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new RefreshTokenCommand(request.RefreshToken), cancellationToken);

        return Ok(result);
    }

    // Se cierra la sesión revocando el refresh token
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new LogoutCommand(request.RefreshToken), cancellationToken);

        return NoContent();
    }

    // Solicita el envío del correo de recuperación de contraseña
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new ForgotPasswordCommand(request.Email), cancellationToken);

        // Siempre respondemos igual sin importar si el correo existe
        return Ok(new { message = "Si el correo está registrado, recibirás instrucciones en breve." });
    }

    // Aplica la nueva contraseña usando el token recibido por correo
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ResetPasswordCommand(
            request.Email,
            request.Token,
            request.NewPassword,
            request.ConfirmNewPassword), cancellationToken);

        return Ok(new { message = "Contraseña actualizada exitosamente." });
    }
}
