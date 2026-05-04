namespace Intelectia.Shared.DTOs.Groups;

public class PagedMessagesDto
{
    public IReadOnlyList<GroupMessageDto> Items { get; set; } = new List<GroupMessageDto>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public bool HasMorePages => Page * PageSize < TotalCount;
}
