using MediatR;
using Intelectia.Shared.DTOs.Auth;

namespace Intelectia.Application.Features.Auth.Commands.Login;

public record LoginCommand(
    string Email,
    string Password
) : IRequest<AuthResponseDto>;
