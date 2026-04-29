using MediatR;
using Microsoft.EntityFrameworkCore;
using Intelectia.Application.Common.Exceptions;
using Intelectia.Application.Common.Interfaces;
using Intelectia.Domain.Enums;
using Intelectia.Domain.Interfaces;
using Intelectia.Domain.Interfaces.Repositories;
using Intelectia.Shared.DTOs.Auth;
using RefreshTokenEntity = Intelectia.Domain.Entities.RefreshToken;

namespace Intelectia.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IApplicationDbContext context,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Cargamos el usuario con perfiles; una sola query, tracking activo
        var user = await _userRepository.GetByEmailWithProfilesAsync(request.Email, cancellationToken);

        // Mismo mensaje para email y contraseña incorrectos; no revelamos cuál falla
        if (user is null || user.AuthProvider != AuthProvider.Local || user.PasswordHash is null)
            throw new UnauthorizedException("Credenciales inválidas.");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Credenciales inválidas.");

        // Revocamos los refresh tokens activos de este usuario directamente en el DbSet
        // para evitar conflictos de tracking al mezclar entidades cargadas y nuevas
        var activeTokens = await _context.RefreshTokens
            .Where(t => t.UserId == user.Id && t.RevokedAt == null && t.UsedAt == null && t.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
            token.RevokedAt = DateTime.UtcNow;

        // Creamos el nuevo refresh token y lo añadimos directamente al DbSet (estado = Added)
        var refreshTokenValue = _tokenService.GenerateRefreshToken();
        await _context.RefreshTokens.AddAsync(new RefreshTokenEntity
        {
            UserId    = user.Id,
            Token     = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        }, cancellationToken);

        // Actualizamos la fecha del último acceso
        user.LastLoginAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var accessToken = _tokenService.GenerateAccessToken(user);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            AccessTokenExpiry = DateTime.UtcNow.AddMinutes(_tokenService.GetAccessTokenExpirationMinutes()),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ProfilePictureUrl = user.ProfilePictureUrl,
                IsStudent = user.StudentProfile is not null,
                IsVendor = user.VendorProfile is not null
            }
        };
    }
}
