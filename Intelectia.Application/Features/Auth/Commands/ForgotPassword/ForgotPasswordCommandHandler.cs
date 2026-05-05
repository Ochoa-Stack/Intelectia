using MediatR;
using Intelectia.Application.Common.Interfaces;
using Intelectia.Domain.Interfaces;
using Intelectia.Domain.Interfaces.Repositories;

namespace Intelectia.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;

    public ForgotPasswordCommandHandler(
        IUserRepository userRepository,
        IEmailService emailService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        // Buscamos el usuario — si no existe respondemos igual para no filtrar información
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null)
            return;

        // Generamos un token de 6 dígitos numéricos simple para el usuario
        var token = new Random().Next(100000, 999999).ToString();

        // Guardamos el token y su fecha de expiración en el usuario
        user.PasswordResetToken = token;
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddMinutes(15);

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Enviamos el correo sin bloquear la respuesta
        _ = _emailService.SendPasswordResetEmailAsync(user.Email, user.FirstName, token, cancellationToken);
    }
}
