using MediatR;
using Intelectia.Application.Common.Exceptions;
using Intelectia.Application.Common.Interfaces;
using Intelectia.Domain.Interfaces;
using Intelectia.Domain.Interfaces.Repositories;

namespace Intelectia.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public ResetPasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        // Buscamos el usuario por correo
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        // Validamos que el token sea correcto y no haya expirado
        if (user is null
            || user.PasswordResetToken != request.Token
            || user.PasswordResetTokenExpiry is null
            || user.PasswordResetTokenExpiry < DateTime.UtcNow)
            throw new ValidationException([
                new FluentValidation.Results.ValidationFailure("Token", "El token no es válido o ha expirado.")
            ]);

        // Actualizamos la contraseña con su nuevo hash
        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);

        // Limpiamos el token para que no pueda usarse de nuevo
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
