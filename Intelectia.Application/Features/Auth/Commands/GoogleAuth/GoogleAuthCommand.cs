using MediatR;
using Intelectia.Shared.DTOs.Auth;

namespace Intelectia.Application.Features.Auth.Commands.GoogleAuth;

public record GoogleAuthCommand(string Code, string RedirectUri) : IRequest<AuthResponseDto>;
