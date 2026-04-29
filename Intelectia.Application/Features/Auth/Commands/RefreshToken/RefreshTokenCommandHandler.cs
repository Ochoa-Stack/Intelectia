using MediatR;
using Microsoft.EntityFrameworkCore;
using Intelectia.Application.Common.Exceptions;
using Intelectia.Application.Common.Interfaces;
using Intelectia.Domain.Entities;
using Intelectia.Domain.Interfaces;
using Intelectia.Domain.Interfaces.Repositories;
using Intelectia.Shared.DTOs.Auth;
using RefreshTokenEntity = Intelectia.Domain.Entities.RefreshToken;

namespace Intelectia.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        ITokenService tokenService,
        IApplicationDbContext context,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Buscamos el refresh token en la base de datos con todos los datos necesarios
        var storedToken = await _context.RefreshTokens
            .Include(r => r.User)
                .ThenInclude(u => u.StudentProfile)
            .Include(r => r.User)
                .ThenInclude(u => u.VendorProfile)
            .Include(r => r.User)
                .ThenInclude(u => u.RefreshTokens)
            .FirstOrDefaultAsync(r => r.Token == request.Token, cancellationToken);

        // Si no existe o ya no es válido rechazamos la petición
        if (storedToken is null || !storedToken.IsActive)
            throw new UnauthorizedException("El refresh token no es válido o ha expirado.");

        // Marcamos el token anterior como usado
        storedToken.UsedAt = DateTime.UtcNow;

        // Generamos un nuevo par de tokens para la sesión renovada
        var newRefreshTokenValue = _tokenService.GenerateRefreshToken();
        storedToken.User.RefreshTokens.Add(new RefreshTokenEntity
        {
            UserId    = storedToken.User.Id,  // explícito para que EF asigne estado Added
            Token     = newRefreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        });

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var accessToken = _tokenService.GenerateAccessToken(storedToken.User);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshTokenValue,
            AccessTokenExpiry = DateTime.UtcNow.AddMinutes(_tokenService.GetAccessTokenExpirationMinutes()),
            User = new UserDto
            {
                Id = storedToken.User.Id,
                Email = storedToken.User.Email,
                FirstName = storedToken.User.FirstName,
                LastName = storedToken.User.LastName,
                ProfilePictureUrl = storedToken.User.ProfilePictureUrl,
                IsStudent = storedToken.User.StudentProfile is not null,
                IsVendor = storedToken.User.VendorProfile is not null
            }
        };
    }
}
