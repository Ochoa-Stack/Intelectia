namespace Intelectia.Shared.DTOs.Groups;

public class GroupMessageDto
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsEdited { get; set; }
    public DateTime CreatedAt { get; set; }
}
