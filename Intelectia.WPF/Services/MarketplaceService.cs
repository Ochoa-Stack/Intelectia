using Intelectia.Shared.DTOs.Marketplace;

namespace Intelectia.WPF.Services;

public class MarketplaceService
{
    private readonly ApiClient _apiClient;

    public MarketplaceService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    // Trae el catálogo paginado con filtros opcionales
    public Task<PagedResult<BookSummaryDto>> GetBooksAsync(
        int page = 1,
        int pageSize = 12,
        string? search = null,
        Guid? categoryId = null,
        string? sortBy = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>
        {
            $"page={page}",
            $"pageSize={pageSize}"
        };

        if (!string.IsNullOrWhiteSpace(search))
            queryParams.Add($"search={Uri.EscapeDataString(search)}");

        if (categoryId.HasValue)
            queryParams.Add($"categoryId={categoryId}");

        if (!string.IsNullOrWhiteSpace(sortBy))
            queryParams.Add($"sortBy={sortBy}");

        var url = $"api/books?{string.Join("&", queryParams)}";
        return _apiClient.GetAsync<PagedResult<BookSummaryDto>>(url, cancellationToken);
    }

    // Trae el detalle de un libro por ID
    public Task<BookDetailDto> GetBookByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _apiClient.GetAsync<BookDetailDto>($"api/books/{id}", cancellationToken);

    // Trae todas las categorías para los filtros
    public Task<List<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
        => _apiClient.GetAsync<List<CategoryDto>>("api/categories", cancellationToken);
}
