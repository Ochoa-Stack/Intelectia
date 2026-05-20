using MediatR;
using Intelectia.Domain.Interfaces;
using Intelectia.Shared.DTOs.Vendor;

namespace Intelectia.Application.Features.Vendor.Queries.GetVendorStats;

public class GetVendorStatsQueryHandler : IRequestHandler<GetVendorStatsQuery, VendorStatsDto>
{
    private readonly IVendorRepository _vendorRepository;

    // Delegamos la obtención de estadísticas complejas a la infraestructura
    public GetVendorStatsQueryHandler(IVendorRepository vendorRepository)
    {
        _vendorRepository = vendorRepository;
    }

    public async Task<VendorStatsDto> Handle(GetVendorStatsQuery request, CancellationToken cancellationToken)
    {
        // Obtenemos las estadísticas del vendedor directamente desde el repositorio
        return await _vendorRepository.GetVendorStatsAsync(request.VendorProfileId, cancellationToken);
    }
}

