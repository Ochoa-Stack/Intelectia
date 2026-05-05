using MediatR;
using Intelectia.Application.Common.Exceptions;
using Intelectia.Application.Common.Interfaces;
using Intelectia.Domain.Entities;
using Intelectia.Domain.Interfaces;
using Intelectia.Domain.Interfaces.Repositories;
using Intelectia.Shared.DTOs.Auth;
using RefreshTokenEntity = Intelectia.Domain.Entities.RefreshToken;

namespace Intelectia.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IEmailService emailService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Verificamos que el correo no esté registrado ya
        var emailExists = await _userRepository.EmailExistsAsync(request.Email, cancellationToken);
        if (emailExists)
            throw new ConflictException("El correo ya está registrado.");

        // Creamos el usuario con la contraseña hasheada
        var user = new User
        {
            Email = request.Email.ToLower(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = _passwordHasher.Hash(request.Password),
            EmailConfirmed = true, // En Fase 8 se activa verificación por correo
            StudentProfile = new StudentProfile() // Todo usuario empieza como estudiante
        };

        // Generamos el refresh token y lo asociamos al usuario
        var refreshTokenValue = _tokenService.GenerateRefreshToken();
        var refreshToken = new RefreshTokenEntity
        {
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };
        user.RefreshTokens.Add(refreshToken);

        // Guardamos el usuario en la base de datos
        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Enviamos el correo de bienvenida sin bloquear la respuesta
        _ = _emailService.SendWelcomeEmailAsync(user.Email, user.FirstName, cancellationToken);

        // Generamos el JWT y construimos la respuesta
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
                IsStudent = user.StudentProfile is not null,
                IsVendor = user.VendorProfile is not null
            }
        };
    }
}
