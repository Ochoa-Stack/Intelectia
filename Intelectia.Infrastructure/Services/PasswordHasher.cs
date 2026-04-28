using Intelectia.Application.Common.Interfaces;

namespace Intelectia.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    // BCrypt incluye el salt automáticamente dentro del hash generado
    public string Hash(string password)
        => BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

    // Compara la contraseña en texto plano contra el hash almacenado
    public bool Verify(string password, string hash)
        => BCrypt.Net.BCrypt.Verify(password, hash);
}
