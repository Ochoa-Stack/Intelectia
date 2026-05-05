namespace Intelectia.WPF.Services;

/* Almacén Singleton del token activo.
AuthService escribe aquí al hacer login/logout.
AuthTokenHandler lee aquí en cada petición HTTP saliente */
public class TokenStore
{
    public string? AccessToken { get; set; }
}
