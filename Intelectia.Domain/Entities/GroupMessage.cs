using Intelectia.Domain.Common;

namespace Intelectia.Domain.Entities;

public class GroupMessage : BaseEntity
{
    // Grupo donde se publicó el mensaje
    public Guid GroupId { get; set; }
    public StudyGroup Group { get; set; } = null!;

    // Usuario que envió el mensaje
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    // Contenido del mensaje
    public string Content { get; set; } = string.Empty;

    // Indica si el mensaje fue editado
    public bool IsEdited { get; set; } = false;
}
