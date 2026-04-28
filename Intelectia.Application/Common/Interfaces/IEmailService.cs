namespace Intelectia.Application.Common.Interfaces;

public interface IEmailService
{
    // Envía el correo de bienvenida al registrarse
    Task SendWelcomeEmailAsync(string toEmail, string firstName, CancellationToken cancellationToken = default);

    // Envía el enlace para restablecer la contraseña
    Task SendPasswordResetEmailAsync(string toEmail, string firstName, string resetToken, CancellationToken cancellationToken = default);
}
