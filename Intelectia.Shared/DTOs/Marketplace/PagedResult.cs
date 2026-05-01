namespace Intelectia.Shared.DTOs.Marketplace;

public class PagedResult<T>
{
    // Los items de la página actual
    public IReadOnlyList<T> Items { get; set; } = new List<T>();

    // Número de página actual
    public int Page { get; set; }

    // Tamaño de cada página
    public int PageSize { get; set; }

    // Total de registros en la base de datos
    public int TotalCount { get; set; }

    // Total de páginas calculado
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    // Indica si hay página siguiente
    public bool HasNextPage => Page < TotalPages;

    // Indica si hay página anterior
    public bool HasPreviousPage => Page > 1;
}
