using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace Intelectia.WPF.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IServiceProvider _serviceProvider;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiClient(HttpClient httpClient, IServiceProvider serviceProvider)
    {
        _httpClient      = httpClient;
        _serviceProvider = serviceProvider;
    }

    public async Task<T> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(endpoint, cancellationToken);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("La respuesta de la API estaba vacía.");
    }

    public async Task<T> PostAsync<T>(string endpoint, object body, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(endpoint, body, cancellationToken);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("La respuesta de la API estaba vacía.");
    }

    public async Task PostAsync(string endpoint, object body, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(endpoint, body, cancellationToken);
        await EnsureSuccessAsync(response);
    }

    public async Task<T> PutAsync<T>(string endpoint, object body, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync(endpoint, body, cancellationToken);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("La respuesta de la API estaba vacía.");
    }

    public async Task PutAsync(string endpoint, object body, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync(endpoint, body, cancellationToken);
        await EnsureSuccessAsync(response);
    }

    public async Task<T> DeleteAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync(endpoint, cancellationToken);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("La respuesta de la API estaba vacía.");
    }

    public async Task DeleteAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync(endpoint, cancellationToken);
        await EnsureSuccessAsync(response);
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;

        // 401 — sesión expirada: limpiamos la sesión y redirigimos al login
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var toast = _serviceProvider.GetService<ToastService>();
            toast?.Warning("Tu sesión expiró. Inicia sesión nuevamente.");

            // Resolvemos AuthService aquí para evitar la dependencia circular en constructor
            var auth = _serviceProvider.GetService<AuthService>();
            auth?.ClearSession();

            var nav = _serviceProvider.GetService<NavigationService>();
            var loginVm = _serviceProvider.GetService<Func<Intelectia.WPF.ViewModels.Auth.LoginViewModel>>();
            if (nav is not null && loginVm is not null)
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    nav.NavigateTo(loginVm()));

            throw new ApiException("Sesión expirada.", 401);
        }

        var content = await response.Content.ReadAsStringAsync();

        try
        {
            using var doc = JsonDocument.Parse(content);
            var message = doc.RootElement.GetProperty("message").GetString()
                ?? "Error desconocido.";
            throw new ApiException(message, (int)response.StatusCode);
        }
        catch (JsonException)
        {
            throw new ApiException(content, (int)response.StatusCode);
        }
    }
}

// Excepción que representa un error con código HTTP de la API
public class ApiException : Exception
{
    public int StatusCode { get; }

    public ApiException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }
}
