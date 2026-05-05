using MediatR;
using Intelectia.Shared.DTOs.Vendor;

namespace Intelectia.Application.Features.Vendor.Commands.BecomeVendor;

public record BecomeVendorCommand(
    Guid UserId,
    string BusinessName,
    string? Description
) : IRequest<VendorProfileDto>;
