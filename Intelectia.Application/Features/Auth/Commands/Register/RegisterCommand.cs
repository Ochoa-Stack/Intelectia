using MediatR;
using Intelectia.Shared.DTOs.Auth;

namespace Intelectia.Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string ConfirmPassword
) : IRequest<AuthResponseDto>;
