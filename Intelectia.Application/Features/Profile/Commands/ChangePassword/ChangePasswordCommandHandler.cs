using MediatR;
using Intelectia.Application.Common.Exceptions;
using Intelectia.Application.Common.Interfaces;
using Intelectia.Domain.Entities;
using Intelectia.Domain.Interfaces;
using Intelectia.Domain.Interfaces.Repositories;

namespace Intelectia.Application.Features.Profile.Commands.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork     _unitOfWork;

    public ChangePasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork     = unitOfWork;
    }

    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdWithProfilesAsync(
            request.UserId, cancellationToken);

        if (user is null)
            throw new NotFoundException(nameof(User), request.UserId);

        // Verificamos que la contraseña actual sea correcta
        if (user.PasswordHash is null ||
            !_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedException("La contraseña actual es incorrecta.");

        // Actualizamos con el nuevo hash
        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
