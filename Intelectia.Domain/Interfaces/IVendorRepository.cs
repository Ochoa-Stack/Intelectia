using Intelectia.Shared.DTOs.Vendor;

namespace Intelectia.Domain.Interfaces;

// Manejamos consultas complejas de negocio fuera del handler
public interface IVendorRepository
{
    Task<VendorStatsDto> GetVendorStatsAsync(Guid vendorProfileId, CancellationToken cancellationToken = default);
}
