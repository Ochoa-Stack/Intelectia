using MediatR;
using Microsoft.EntityFrameworkCore;
using Intelectia.Application.Common.Exceptions;
using Intelectia.Application.Common.Interfaces;
using Intelectia.Domain.Entities;
using Intelectia.Domain.Interfaces;
using Intelectia.Shared.DTOs.Vendor;

namespace Intelectia.Application.Features.Vendor.Commands.BecomeVendor;

public class BecomeVendorCommandHandler : IRequestHandler<BecomeVendorCommand, VendorProfileDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public BecomeVendorCommandHandler(IApplicationDbContext context, IUnitOfWork unitOfWork)
    {
        _context    = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<VendorProfileDto> Handle(
        BecomeVendorCommand request, CancellationToken cancellationToken)
    {
        // Verificamos que el usuario exista
        var user = await _context.Users
            .Include(u => u.VendorProfile)
            .FirstOrDefaultAsync(u => u.Id == request.UserId && !u.IsDeleted, cancellationToken);

        if (user is null)
            throw new NotFoundException(nameof(User), request.UserId);

        // Si ya tiene perfil de vendedor activo no hacemos nada
        if (user.VendorProfile is not null && user.VendorProfile.IsActive)
            throw new ConflictException("Ya tienes un perfil de vendedor activo.");

        // Creamos el perfil de vendedor
        var vendorProfile = new VendorProfile
        {
            UserId       = request.UserId,
            BusinessName = request.BusinessName,
            Description  = request.Description,
            IsActive     = true,
            ActivatedAt  = DateTime.UtcNow
        };

        await _context.VendorProfiles.AddAsync(vendorProfile, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new VendorProfileDto
        {
            Id           = vendorProfile.Id,
            BusinessName = vendorProfile.BusinessName,
            Description  = vendorProfile.Description,
            IsActive     = vendorProfile.IsActive,
            ActivatedAt  = vendorProfile.ActivatedAt
        };
    }
}
