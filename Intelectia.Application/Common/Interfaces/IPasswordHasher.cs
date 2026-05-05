namespace Intelectia.Application.Common.Interfaces;

public interface IPasswordHasher
{
    // Convierte la contraseña en texto plano a su versión hasheada
    string Hash(string password);

    // Compara una contraseña en texto plano con su hash almacenado
    bool Verify(string password, string hash);
}
