namespace Intelectia.Shared.DTOs.Groups;

public class CreateGroupRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPublic { get; set; } = true;
}
