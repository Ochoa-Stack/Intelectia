namespace Intelectia.Shared.DTOs.Library;

public class GenerateCitationRequest
{
    public Guid BookId { get; set; }
    public string Format { get; set; } = "APA";
    public int? PageNumber { get; set; }
}
