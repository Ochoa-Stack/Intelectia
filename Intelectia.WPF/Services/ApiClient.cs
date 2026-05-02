using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace Intelectia.WPF.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;

    // Opciones de deserialización; nombres de propiedades en camelCase como los devuelve la API
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Hace un GET y deserializa la respuesta
    public async Task<T> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(endpoint, cancellationToken);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("La respuesta de la API estaba vacía.");
    }

    // Hace un POST y deserializa la respuesta al tipo pedido
    public async Task<T> PostAsync<T>(
        string endpoint, object body, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(endpoint, body, cancellationToken);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("La respuesta de la API estaba vacía.");
    }

    // Hace un POST sin devolver cuerpo
    public async Task PostAsync(
        string endpoint, object body, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(endpoint, body, cancellationToken);
        await EnsureSuccessAsync(response);
    }

    // Hace un DELETE y deserializa la respuesta
    public async Task<T> DeleteAsync<T>(
        string endpoint, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync(endpoint, cancellationToken);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("La respuesta de la API estaba vacía.");
    }

    // Hace un PUT sin devolver cuerpo
    public async Task PutAsync(
        string endpoint, object body, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync(endpoint, body, cancellationToken);
        await EnsureSuccessAsync(response);
    }

    // Lee el mensaje de error estructurado que devuelve GlobalExceptionMiddleware
    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;

        var content = await response.Content.ReadAsStringAsync();

        try
        {
            using var doc = JsonDocument.Parse(content);
            var message = doc.RootElement
                .GetProperty("message")
                .GetString() ?? "Error desconocido.";
            throw new ApiException(message, (int)response.StatusCode);
        }
        catch (JsonException)
        {
            // Si el body no es JSON estructurado lo usamos como mensaje crudo
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
