using MediatR;

namespace Intelectia.Application.Features.Auth.Commands.ForgotPassword;

// Solo necesita el correo del usuario
public record ForgotPasswordCommand(string Email) : IRequest;
