using System.Diagnostics;
using System.Net;
using System.Text;
using System.Web;
using Intelectia.Shared.DTOs.Auth;

namespace Intelectia.WPF.Services;

public class GoogleAuthService
{
    private readonly ApiClient _apiClient;
    private readonly TokenStore _tokenStore;
    private readonly CredentialService _credentialService;

    // Puerto local para capturar el callback de Google
    private const int CallbackPort = 5100;
    private static readonly string RedirectUri = $"http://localhost:{CallbackPort}/auth/callback";

    public GoogleAuthService(
        ApiClient apiClient,
        TokenStore tokenStore,
        CredentialService credentialService)
    {
        _apiClient         = apiClient;
        _tokenStore        = tokenStore;
        _credentialService = credentialService;
    }

    // Inicia el flujo OAuth abriendo el navegador y esperando el callback
    public async Task<AuthResponseDto> AuthenticateAsync(CancellationToken cancellationToken = default)
    {
        // Generamos un state aleatorio para prevenir CSRF
        var state = Guid.NewGuid().ToString("N");

        // Pedimos al backend la URL de autorización de Google
        var authUrlResponse = await _apiClient.GetAsync<GoogleAuthUrlDto>(
            $"api/auth/google/url?redirectUri={Uri.EscapeDataString(RedirectUri)}&state={state}",
            cancellationToken);

        // Iniciamos el listener ANTES de abrir el browser para no perder el callback
        var listenTask = ListenForCallbackAsync(state, cancellationToken);

        // Pausa mínima para que el HttpListener esté activo
        await Task.Delay(300, cancellationToken);

        // Abrimos el navegador del sistema con la URL de Google
        Process.Start(new ProcessStartInfo
        {
            FileName        = authUrlResponse.AuthUrl,
            UseShellExecute = true
        });

        // Esperamos el código del callback
        var code = await listenTask;

        // Traemos la app al frente inmediatamente después del OAuth
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            var win = System.Windows.Application.Current.MainWindow;
            if (win is not null)
            {
                win.WindowState = System.Windows.WindowState.Normal;
                win.Activate();
                win.Focus();
            }
        });

        // Canjeamos el code por tokens de Intelectia
        var response = await _apiClient.PostAsync<AuthResponseDto>(
            "api/auth/google/callback",
            new GoogleCallbackRequest
            {
                Code        = code,
                RedirectUri = RedirectUri
            },
            cancellationToken);

        // Guardamos la sesión
        _tokenStore.AccessToken = response.AccessToken;
        _credentialService.SaveRefreshToken(response.RefreshToken);

        return response;
    }

    // Levanta un listener HTTP temporal para capturar el código de autorización
    private static async Task<string> ListenForCallbackAsync(
        string expectedState, CancellationToken cancellationToken)
    {
        using var listener = new HttpListener();
        listener.Prefixes.Add($"http://localhost:{CallbackPort}/auth/callback/");
        listener.Start();

        // Esperamos la petición con timeout de 5 minutos
        var contextTask = listener.GetContextAsync();
        var timeoutTask = Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
        var completed   = await Task.WhenAny(contextTask, timeoutTask);

        if (completed == timeoutTask)
        {
            listener.Stop();
            throw new TimeoutException("El tiempo de autenticación con Google expiró.");
        }

        var context = await contextTask;
        var query   = context.Request.QueryString;
        var code    = query["code"]  ?? throw new InvalidOperationException("No se recibió código de Google.");
        var state   = query["state"] ?? string.Empty;

        // Leemos el HTML desde el recurso embebido
        var assembly     = System.Reflection.Assembly.GetExecutingAssembly();
        var resourceName = "Intelectia.WPF.Resources.OAuthCallback.html";
        string htmlContent;

        using (var stream = assembly.GetManifestResourceStream(resourceName))
        using (var reader = new System.IO.StreamReader(stream
            ?? throw new InvalidOperationException("Recurso OAuthCallback.html no encontrado.")))
        {
            htmlContent = await reader.ReadToEndAsync();
        }

        var responseBytes = Encoding.UTF8.GetBytes(htmlContent);

        context.Response.ContentType     = "text/html; charset=utf-8";
        context.Response.ContentLength64 = responseBytes.Length;
        await context.Response.OutputStream.WriteAsync(responseBytes, cancellationToken);
        context.Response.OutputStream.Close();

        listener.Stop();

        // Verificamos el state para prevenir CSRF
        if (state != expectedState)
            throw new InvalidOperationException("State de OAuth inválido.");

        return code;
    }
}

// DTO para la URL de autorización de Google
public class GoogleAuthUrlDto
{
    public string AuthUrl { get; set; } = string.Empty;
}

// DTO para el callback de Google
public class GoogleCallbackRequest
{
    public string Code { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
}
