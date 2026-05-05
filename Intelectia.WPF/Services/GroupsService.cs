using Intelectia.Shared.DTOs.Groups;

namespace Intelectia.WPF.Services;

public class GroupsService
{
    private readonly ApiClient _apiClient;

    public GroupsService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<List<GroupDto>> GetMyGroupsAsync(CancellationToken ct = default)
        => _apiClient.GetAsync<List<GroupDto>>("api/groups/my", ct);

    public Task<List<GroupDto>> GetPublicGroupsAsync(
        string? search = null, CancellationToken ct = default)
    {
        var url = string.IsNullOrWhiteSpace(search)
            ? "api/groups/public"
            : $"api/groups/public?search={Uri.EscapeDataString(search)}";
        return _apiClient.GetAsync<List<GroupDto>>(url, ct);
    }

    public Task<GroupDto> CreateGroupAsync(
        CreateGroupRequest request, CancellationToken ct = default)
        => _apiClient.PostAsync<GroupDto>("api/groups", request, ct);

    public Task JoinGroupAsync(Guid groupId, CancellationToken ct = default)
        => _apiClient.PostAsync($"api/groups/{groupId}/join", new { }, ct);

    public Task LeaveGroupAsync(Guid groupId, CancellationToken ct = default)
        => _apiClient.DeleteAsync($"api/groups/{groupId}/leave", ct);

    public Task<PagedMessagesDto> GetMessagesAsync(
        Guid groupId, int page = 1, int pageSize = 30, CancellationToken ct = default)
        => _apiClient.GetAsync<PagedMessagesDto>(
            $"api/groups/{groupId}/messages?page={page}&pageSize={pageSize}", ct);
}
