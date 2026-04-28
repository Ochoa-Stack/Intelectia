using MediatR;
using Intelectia.Shared.DTOs.Auth;

namespace Intelectia.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(
    string Token
) : IRequest<AuthResponseDto>;
