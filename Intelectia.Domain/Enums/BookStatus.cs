namespace Intelectia.Domain.Enums;

public enum BookStatus
{
    // En revisión antes de publicarse
    Draft = 1,

    // Visible y disponible para compra
    Active = 2,

    // Temporalmente oculto por el vendedor
    Inactive = 3
}
