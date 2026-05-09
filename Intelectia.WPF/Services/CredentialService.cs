using System.Net;
using AdysTech.CredentialManager;

namespace Intelectia.WPF.Services;

public class CredentialService
{
    // Clave bajo la que se guarda el refresh token en Windows Credential Manager
    private const string CredentialKey = "Intelectia_RefreshToken";

    // Guarda el refresh token cifrado en el almacÃ©n de Windows
    public void SaveRefreshToken(string token)
    {
        try
        {
            var credential = new NetworkCredential(CredentialKey, token);
            CredentialManager.SaveCredentials(CredentialKey, credential);
        }
        catch
        {
            // Si falla el guardado la app sigue funcionando; solo pierde la persistencia
        }
    }

    // Recupera el refresh token guardado; null si no existe o fallÃ³
    public string? LoadRefreshToken()
    {
        try
        {
            var credential = CredentialManager.GetCredentials(CredentialKey);
            return credential?.Password;
        }
        catch
        {
            return null;
        }
    }

    // Elimina el refresh token al cerrar sesiÃ³n
    public void DeleteRefreshToken()
    {
        try
        {
            CredentialManager.RemoveCredentials(CredentialKey);
        }
        catch (System.Exception ex) { System.Diagnostics.Debug.WriteLine(ex); }
    }
}

