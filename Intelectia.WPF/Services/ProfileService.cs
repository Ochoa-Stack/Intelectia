using Intelectia.Shared.DTOs.Profile;

namespace Intelectia.WPF.Services;

public class ProfileService
{
    private readonly ApiClient _apiClient;

    public ProfileService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<UserProfileDto> GetProfileAsync(CancellationToken ct = default)
        => _apiClient.GetAsync<UserProfileDto>("api/users/me", ct);

    public Task<UserProfileDto> UpdateProfileAsync(
        UpdateProfileRequest request, CancellationToken ct = default)
        => _apiClient.PutAsync<UserProfileDto>("api/users/me", request, ct);

    public Task ChangePasswordAsync(
        ChangePasswordRequest request, CancellationToken ct = default)
        => _apiClient.PutAsync("api/users/me/password", request, ct);
}
