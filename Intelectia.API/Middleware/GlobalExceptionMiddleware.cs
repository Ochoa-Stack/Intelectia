using System.Net;
using System.Text.Json;
using Intelectia.Application.Common.Exceptions;
using ValidationException = Intelectia.Application.Common.Exceptions.ValidationException;

namespace Intelectia.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción no controlada: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        // Mapeamos cada tipo de excepción al código HTTP correspondiente
        var (statusCode, message, errors) = exception switch
        {
            ValidationException ve   => (HttpStatusCode.BadRequest,          ve.Message,  (object?)ve.Errors),
            NotFoundException nfe    => (HttpStatusCode.NotFound,            nfe.Message, null),
            ConflictException ce     => (HttpStatusCode.Conflict,            ce.Message,  null),
            UnauthorizedException ue => (HttpStatusCode.Unauthorized,        ue.Message,  null),
            _                        => (HttpStatusCode.InternalServerError, "Ocurrió un error inesperado.", null)
        };

        context.Response.StatusCode = (int)statusCode;

        var body = JsonSerializer.Serialize(
            new { status = (int)statusCode, message, errors },
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        await context.Response.WriteAsync(body);
    }
}
