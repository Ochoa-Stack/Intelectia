namespace Intelectia.Shared.DTOs.Vendor;

public class VendorProfileDto
{
    public Guid Id { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ActivatedAt { get; set; }
}
