using MediatR;
using Intelectia.Application.Common.Exceptions;
using Intelectia.Application.Common.Interfaces;
using Intelectia.Domain.Entities;
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
    private readonly IUnitOfWork _unitOfWork;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Buscamos el usuario por correo
        var userByEmail = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        // Cargamos el usuario completo con perfiles y tokens si existe
        var user = userByEmail is not null
            ? await _userRepository.GetByIdWithProfilesAsync(userByEmail.Id, cancellationToken)
            : null;

        // Usamos el mismo mensaje para email incorrecto y contraseña incorrecta
        // para no dar pistas sobre qué campo falló
        if (user is null || user.AuthProvider != AuthProvider.Local || user.PasswordHash is null)
            throw new UnauthorizedException("Credenciales inválidas.");

        var passwordValid = _passwordHasher.Verify(request.Password, user.PasswordHash);
        if (!passwordValid)
            throw new UnauthorizedException("Credenciales inválidas.");

        // Revocamos los refresh tokens anteriores que siguen activos
        foreach (var token in user.RefreshTokens.Where(t => t.IsActive))
            token.RevokedAt = DateTime.UtcNow;

        // Generamos un nuevo refresh token para esta sesión
        var refreshTokenValue = _tokenService.GenerateRefreshToken();
        user.RefreshTokens.Add(new RefreshTokenEntity
        {
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        });

        // Actualizamos la fecha del último acceso
        user.LastLoginAt = DateTime.UtcNow;
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Generamos el JWT y devolvemos la respuesta
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
