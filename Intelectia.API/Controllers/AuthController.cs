using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Intelectia.Application.Features.Auth.Commands.ForgotPassword;
using Intelectia.Application.Features.Auth.Commands.Login;
using Intelectia.Application.Features.Auth.Commands.Logout;
using Intelectia.Application.Features.Auth.Commands.RefreshToken;
using Intelectia.Application.Features.Auth.Commands.Register;
using Intelectia.Application.Features.Auth.Commands.ResetPassword;
using Intelectia.Application.Features.Auth.Commands.GoogleAuth;
using Intelectia.Shared.DTOs.Auth;

namespace Intelectia.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("AuthPolicy")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;

    public AuthController(IMediator mediator, IConfiguration configuration)
    {
        _mediator      = mediator;
        _configuration = configuration;
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

    // Devuelve la URL de autorización de Google para que el cliente WPF la abra en el navegador
    [HttpGet("google/url")]
    [AllowAnonymous]
    public IActionResult GetGoogleAuthUrl(
        [FromQuery] string redirectUri,
        [FromQuery] string state)
    {
        var clientId = _configuration["ExternalServices:Google:ClientId"]
            ?? throw new InvalidOperationException("Google ClientId no configurado.");

        // Construimos la URL de autorización de Google OAuth 2.0
        var authUrl = "https://accounts.google.com/o/oauth2/v2/auth" +
            $"?client_id={Uri.EscapeDataString(clientId)}" +
            $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
            $"&response_type=code" +
            $"&scope={Uri.EscapeDataString("openid email profile")}" +
            $"&state={Uri.EscapeDataString(state)}" +
            $"&access_type=offline";

        return Ok(new { authUrl });
    }

    // Canjea el code de Google por tokens de Intelectia
    [HttpPost("google/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleCallback(
        [FromBody] GoogleCallbackDto request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GoogleAuthCommand(request.Code, request.RedirectUri), cancellationToken);
        return Ok(result);
    }
}

public record GoogleCallbackDto(string Code, string RedirectUri);
