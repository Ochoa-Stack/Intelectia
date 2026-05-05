using Intelectia.Shared.DTOs.Auth;

namespace Intelectia.WPF.Services;

public class AuthService
{
    private readonly ApiClient _apiClient;
    private readonly TokenStore _tokenStore;
    private readonly CredentialService _credentialService;

    public AuthResponseDto? CurrentSession { get; private set; }
    public bool IsAuthenticated => CurrentSession is not null;

    public AuthService(
        ApiClient apiClient,
        TokenStore tokenStore,
        CredentialService credentialService)
    {
        _apiClient         = apiClient;
        _tokenStore        = tokenStore;
        _credentialService = credentialService;
    }

    public async Task<AuthResponseDto> RegisterAsync(
        RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _apiClient.PostAsync<AuthResponseDto>(
            "api/auth/register", request, cancellationToken);
        SetSession(response);
        return response;
    }

    public async Task<AuthResponseDto> LoginAsync(
        LoginRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _apiClient.PostAsync<AuthResponseDto>(
            "api/auth/login", request, cancellationToken);
        SetSession(response);
        return response;
    }

    // Intenta restaurar la sesión desde el Credential Manager al arrancar la app
    public async Task<bool> TryRestoreSessionAsync(CancellationToken cancellationToken = default)
    {
        var savedToken = _credentialService.LoadRefreshToken();
        if (savedToken is null) return false;

        try
        {
            var response = await _apiClient.PostAsync<AuthResponseDto>(
                "api/auth/refresh",
                new RefreshTokenRequest { RefreshToken = savedToken },
                cancellationToken);

            SetSession(response);
            return true;
        }
        catch
        {
            // El token guardado ya no es válido; lo limpiamos
            _credentialService.DeleteRefreshToken();
            return false;
        }
    }

    public async Task<bool> RefreshSessionAsync(CancellationToken cancellationToken = default)
    {
        if (CurrentSession is null) return false;

        try
        {
            var response = await _apiClient.PostAsync<AuthResponseDto>(
                "api/auth/refresh",
                new RefreshTokenRequest { RefreshToken = CurrentSession.RefreshToken },
                cancellationToken);
            SetSession(response);
            return true;
        }
        catch
        {
            ClearSession();
            return false;
        }
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        if (CurrentSession is null) return;

        try
        {
            await _apiClient.PostAsync(
                "api/auth/logout",
                new LogoutRequest { RefreshToken = CurrentSession.RefreshToken },
                cancellationToken);
        }
        finally
        {
            ClearSession();
        }
    }

    public async Task ForgotPasswordAsync(
        string email, CancellationToken cancellationToken = default)
        => await _apiClient.PostAsync(
            "api/auth/forgot-password",
            new ForgotPasswordRequest { Email = email },
            cancellationToken);

    public async Task ResetPasswordAsync(
        ResetPasswordRequest request, CancellationToken cancellationToken = default)
        => await _apiClient.PostAsync("api/auth/reset-password", request, cancellationToken);

    // Registra una sesión construida externamente; usado por Google OAuth
    public void SetSessionFromExternal(AuthResponseDto session)
        => SetSession(session);

    // Guarda la sesión, el token en memoria y el refresh token en Credential Manager
    private void SetSession(AuthResponseDto session)
    {
        CurrentSession          = session;
        _tokenStore.AccessToken = session.AccessToken;
        _credentialService.SaveRefreshToken(session.RefreshToken);
    }

    // Limpia la sesión en memoria y elimina el token del Credential Manager
    public void ClearSession()
    {
        CurrentSession          = null;
        _tokenStore.AccessToken = null;
        _credentialService.DeleteRefreshToken();
    }
}
