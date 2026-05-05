using MediatR;

namespace Intelectia.Application.Features.Auth.Commands.Logout;

// Recibe el refresh token activo para revocarlo
public record LogoutCommand(string RefreshToken) : IRequest;
