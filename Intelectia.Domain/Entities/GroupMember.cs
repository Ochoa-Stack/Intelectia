using Intelectia.Domain.Common;
using Intelectia.Domain.Enums;

namespace Intelectia.Domain.Entities;

public class GroupMember : BaseEntity
{
    // Grupo al que pertenece este miembro
    public Guid GroupId { get; set; }
    public StudyGroup Group { get; set; } = null!;

    // Usuario miembro
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    // Rol del usuario dentro del grupo
    public GroupMemberRole Role { get; set; } = GroupMemberRole.Member;

    // Fecha en que se unió al grupo
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
