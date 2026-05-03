using Intelectia.Shared.DTOs.Vendor;

namespace Intelectia.WPF.Services;

public class VendorService
{
    private readonly ApiClient _apiClient;

    public VendorService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    // Activa el perfil de vendedor
    public Task<VendorProfileDto> BecomeVendorAsync(
        BecomeVendorRequest request, CancellationToken cancellationToken = default)
        => _apiClient.PostAsync<VendorProfileDto>(
            "api/vendors/me/become-vendor", request, cancellationToken);

    // Trae los libros publicados por el vendedor
    public Task<List<VendorBookDto>> GetMyBooksAsync(CancellationToken cancellationToken = default)
        => _apiClient.GetAsync<List<VendorBookDto>>("api/vendors/me/books", cancellationToken);

    // Publica un libro nuevo
    public Task<VendorBookDto> PublishBookAsync(
        PublishBookRequest request, CancellationToken cancellationToken = default)
        => _apiClient.PostAsync<VendorBookDto>("api/vendors/me/books", request, cancellationToken);

    // Trae las estadísticas del vendedor
    public Task<VendorStatsDto> GetStatsAsync(CancellationToken cancellationToken = default)
        => _apiClient.GetAsync<VendorStatsDto>("api/vendors/me/stats", cancellationToken);
}
