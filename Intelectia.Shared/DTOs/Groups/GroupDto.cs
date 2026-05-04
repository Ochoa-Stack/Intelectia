namespace Intelectia.Shared.DTOs.Groups;

public class GroupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPublic { get; set; }
    public int MemberCount { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Rol del usuario en este grupo; null si no es miembro
    public string? UserRole { get; set; }
}
