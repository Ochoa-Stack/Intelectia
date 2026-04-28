using MediatR;

namespace Intelectia.Application.Features.Auth.Commands.ResetPassword;

public record ResetPasswordCommand(
    string Email,
    string Token,
    string NewPassword,
    string ConfirmNewPassword
) : IRequest;
