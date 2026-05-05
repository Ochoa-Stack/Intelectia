using Intelectia.Domain.Common;

namespace Intelectia.Domain.Entities;

public class StudyGroup : BaseEntity
{
    // Nombre visible del grupo
    public string Name { get; set; } = string.Empty;

    // Descripción del propósito del grupo
    public string? Description { get; set; }

    // Usuario que creó el grupo (es Admin automáticamente)
    public Guid CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;

    // Indica si el grupo es público o solo por invitación
    public bool IsPublic { get; set; } = true;

    // Miembros del grupo
    public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();

    // Mensajes del chat del grupo
    public ICollection<GroupMessage> Messages { get; set; } = new List<GroupMessage>();

    // Total de miembros (calculado al consultar)
    public int MemberCount => Members.Count(m => !m.IsDeleted);
}
