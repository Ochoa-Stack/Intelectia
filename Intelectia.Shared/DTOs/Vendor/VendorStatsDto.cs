namespace Intelectia.Shared.DTOs.Vendor;

public class VendorStatsDto
{
    // Total de libros publicados por el vendedor
    public int TotalBooks { get; set; }

    // Total de ventas confirmadas
    public int TotalSales { get; set; }

    // Ingresos totales acumulados
    public decimal TotalRevenue { get; set; }

    // Promedio de calificación de todos sus libros
    public double AverageRating { get; set; }

    // Libro más vendido
    public string? TopBookTitle { get; set; }
}
