using MediatR;
using Intelectia.Shared.DTOs.Vendor;

namespace Intelectia.Application.Features.Vendor.Queries.GetVendorStats;

public record GetVendorStatsQuery(Guid VendorProfileId) : IRequest<VendorStatsDto>;
