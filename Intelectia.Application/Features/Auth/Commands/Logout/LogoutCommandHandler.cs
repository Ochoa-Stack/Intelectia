using MediatR;
using Intelectia.Domain.Interfaces;
using Intelectia.Domain.Interfaces.Repositories;

namespace Intelectia.Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutCommandHandler(IRefreshTokenRepository refreshTokenRepository, IUnitOfWork unitOfWork)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        // Buscamos el refresh token que el cliente quiere invalidar
        var token = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);

        // Si no existe simplemente ignoramos — el resultado es el mismo: sesión cerrada
        if (token is null || !token.IsActive)
            return;

        // Revocamos el token para que no pueda usarse de nuevo
        token.RevokedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
