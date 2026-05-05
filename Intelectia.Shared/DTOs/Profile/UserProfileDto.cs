namespace Intelectia.Shared.DTOs.Profile;

public class UserProfileDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public bool IsStudent { get; set; }
    public bool IsVendor { get; set; }
    public DateTime? LastLoginAt { get; set; }
}
