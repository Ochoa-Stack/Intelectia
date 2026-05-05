namespace Intelectia.Shared.DTOs.Vendor;

public class BecomeVendorRequest
{
    // Nombre comercial del vendedor
    public string BusinessName { get; set; } = string.Empty;

    // Descripción pública del perfil de vendedor
    public string? Description { get; set; }
}
