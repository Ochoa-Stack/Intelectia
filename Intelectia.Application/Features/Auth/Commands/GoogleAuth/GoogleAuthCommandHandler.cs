using System.Net.Http.Json;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Configuration;
using Intelectia.Application.Common.Interfaces;
using Intelectia.Domain.Entities;
using Intelectia.Domain.Enums;
using Intelectia.Domain.Interfaces;
using Intelectia.Domain.Interfaces.Repositories;
using Intelectia.Shared.DTOs.Auth;
using RefreshTokenEntity = Intelectia.Domain.Entities.RefreshToken;

namespace Intelectia.Application.Features.Auth.Commands.GoogleAuth;

public class GoogleAuthCommandHandler : IRequestHandler<GoogleAuthCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService   _tokenService;
    private readonly IUnitOfWork     _unitOfWork;
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration  _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public GoogleAuthCommandHandler(
        IUserRepository userRepository,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        IApplicationDbContext context,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _userRepository    = userRepository;
        _tokenService      = tokenService;
        _unitOfWork        = unitOfWork;
        _context           = context;
        _configuration     = configuration;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<AuthResponseDto> Handle(
        GoogleAuthCommand request, CancellationToken cancellationToken)
    {
        // Canjeamos el code por un access token de Google
        var googleTokens = await ExchangeCodeForTokensAsync(request.Code, request.RedirectUri);

        // Obtenemos los datos del usuario desde Google
        var googleUser = await GetGoogleUserInfoAsync(googleTokens.AccessToken);

        // Buscamos el usuario por su Google ID o email
        var user = await _userRepository.GetByGoogleIdAsync(googleUser.Id, cancellationToken)
                ?? await _userRepository.GetByEmailAsync(googleUser.Email, cancellationToken);

        if (user is null)
        {
            // Creamos el usuario si no existe
            user = new User
            {
                Email          = googleUser.Email.ToLower(),
                FirstName      = googleUser.GivenName,
                LastName       = googleUser.FamilyName,
                GoogleId       = googleUser.Id,
                AuthProvider   = AuthProvider.Google,
                EmailConfirmed = true,
                ProfilePictureUrl = googleUser.Picture,
                StudentProfile = new StudentProfile()
            };

            await _userRepository.AddAsync(user, cancellationToken);
        }
        else
        {
            // Vinculamos la cuenta de Google si el usuario existía con email/password
            if (user.GoogleId is null)
                user.GoogleId = googleUser.Id;

            user.LastLoginAt = DateTime.UtcNow;
            _userRepository.Update(user);
        }

        // Generamos refresh token y lo asociamos al usuario
        var refreshTokenValue = _tokenService.GenerateRefreshToken();
        await _context.RefreshTokens.AddAsync(new RefreshTokenEntity
        {
            UserId    = user.Id,
            Token     = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Recargamos el usuario con sus perfiles para generar el JWT
        var fullUser = await _userRepository.GetByIdWithProfilesAsync(user.Id, cancellationToken)
            ?? user;

        var accessToken = _tokenService.GenerateAccessToken(fullUser);

        return new AuthResponseDto
        {
            AccessToken       = accessToken,
            RefreshToken      = refreshTokenValue,
            AccessTokenExpiry = DateTime.UtcNow.AddMinutes(_tokenService.GetAccessTokenExpirationMinutes()),
            User = new UserDto
            {
                Id                = fullUser.Id,
                Email             = fullUser.Email,
                FirstName         = fullUser.FirstName,
                LastName          = fullUser.LastName,
                ProfilePictureUrl = fullUser.ProfilePictureUrl,
                IsStudent         = fullUser.StudentProfile is not null,
                IsVendor          = fullUser.VendorProfile is not null
            }
        };
    }

    // Canjea el authorization code por tokens de Google
    private async Task<GoogleTokenResponse> ExchangeCodeForTokensAsync(
        string code, string redirectUri)
    {
        var client = _httpClientFactory.CreateClient();

        var tokenRequest = new Dictionary<string, string>
        {
            ["code"]          = code,
            ["client_id"]     = _configuration["ExternalServices:Google:ClientId"]!,
            ["client_secret"] = _configuration["ExternalServices:Google:ClientSecret"]!,
            ["redirect_uri"]  = redirectUri,
            ["grant_type"]    = "authorization_code"
        };

        var response = await client.PostAsync(
            "https://oauth2.googleapis.com/token",
            new FormUrlEncodedContent(tokenRequest));

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<GoogleTokenResponse>()
            ?? throw new InvalidOperationException("Respuesta vacía de Google OAuth.");
    }

    // Obtiene los datos del usuario desde la API de Google
    private async Task<GoogleUserInfo> GetGoogleUserInfoAsync(string accessToken)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<GoogleUserInfo>()
            ?? throw new InvalidOperationException("No se pudo obtener información del usuario de Google.");
    }
}

// Respuesta de tokens de Google
public record GoogleTokenResponse(
    [property: System.Text.Json.Serialization.JsonPropertyName("access_token")]
    string AccessToken,
    [property: System.Text.Json.Serialization.JsonPropertyName("refresh_token")]
    string? RefreshToken
);

// Datos del usuario de Google
public record GoogleUserInfo(
    [property: System.Text.Json.Serialization.JsonPropertyName("id")]
    string Id,
    [property: System.Text.Json.Serialization.JsonPropertyName("email")]
    string Email,
    [property: System.Text.Json.Serialization.JsonPropertyName("given_name")]
    string GivenName,
    [property: System.Text.Json.Serialization.JsonPropertyName("family_name")]
    string FamilyName,
    [property: System.Text.Json.Serialization.JsonPropertyName("picture")]
    string? Picture
);
