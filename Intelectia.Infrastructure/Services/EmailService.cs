using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using Intelectia.Application.Common.Interfaces;

namespace Intelectia.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string firstName, CancellationToken cancellationToken = default)
    {
        var subject = "¡Bienvenido a Intelectia!";
        var body = $@"
            <h2>Hola, {firstName}.</h2>
            <p>Tu cuenta en Intelectia ha sido creada exitosamente.</p>
            <p>Ya puedes explorar el catálogo y gestionar tu biblioteca personal.</p>
        ";

        await SendAsync(toEmail, subject, body, cancellationToken);
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string firstName, string resetToken, CancellationToken cancellationToken = default)
    {
        var subject = "Restablecer contraseña — Intelectia";
        var body = $@"
            <h2>Hola, {firstName}.</h2>
            <p>Recibimos una solicitud para restablecer tu contraseña.</p>
            <p>Usa el siguiente código: <strong>{resetToken}</strong></p>
            <p>Este código expira en 15 minutos. Si no solicitaste el cambio, ignora este correo.</p>
        ";

        await SendAsync(toEmail, subject, body, cancellationToken);
    }

    // Método central que construye y envía el correo vía SMTP
    private async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        try
        {
            var fromName    = _configuration["ExternalServices:Email:FromName"] ?? "Intelectia";
            var fromAddress = _configuration["ExternalServices:Email:FromAddress"]
                ?? throw new InvalidOperationException("Email FromAddress no está configurado.");
            var smtpHost    = _configuration["ExternalServices:Email:SmtpHost"]
                ?? throw new InvalidOperationException("Email SmtpHost no está configurado.");
            var smtpPort    = int.Parse(_configuration["ExternalServices:Email:SmtpPort"] ?? "587");
            var smtpUser    = _configuration["ExternalServices:Email:SmtpUser"]
                ?? throw new InvalidOperationException("Email SmtpUser no está configurado.");
            var smtpPass    = _configuration["ExternalServices:Email:SmtpPassword"]
                ?? throw new InvalidOperationException("Email SmtpPassword no está configurado.");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromAddress));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            // Construimos el cuerpo en formato HTML
            var builder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls, cancellationToken);
            await client.AuthenticateAsync(smtpUser, smtpPass, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception ex)
        {
            // Registramos el error sin interrumpir el flujo principal
            _logger.LogError(ex, "Error al enviar correo a {Email}", toEmail);
        }
    }
}
