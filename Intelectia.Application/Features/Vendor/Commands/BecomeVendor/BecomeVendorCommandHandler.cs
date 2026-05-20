using MediatR;
using Intelectia.Application.Common.Exceptions;
using Intelectia.Domain.Entities;
using Intelectia.Domain.Interfaces;
using Intelectia.Shared.DTOs.Vendor;

namespace Intelectia.Application.Features.Vendor.Commands.BecomeVendor;

public class BecomeVendorCommandHandler : IRequestHandler<BecomeVendorCommand, VendorProfileDto>
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<VendorProfile> _vendorRepository;
    private readonly IUnitOfWork _unitOfWork;

    // Inyectamos los repositorios genéricos necesarios
    public BecomeVendorCommandHandler(
        IRepository<User> userRepository, 
        IRepository<VendorProfile> vendorRepository, 
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _vendorRepository = vendorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<VendorProfileDto> Handle(BecomeVendorCommand request, CancellationToken cancellationToken)
    {
        // Buscamos el usuario en el almacén de datos
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
            throw new NotFoundException(nameof(User), request.UserId);

        // Verificamos si ya existe un perfil de vendedor activo para el usuario
        var existingProfile = _vendorRepository.Find(v => v.UserId == request.UserId && v.IsActive).FirstOrDefault();
        
        if (existingProfile is not null)
            throw new ConflictException("Ya tienes un perfil de vendedor activo.");

        var vendorProfile = new VendorProfile
        {
            UserId = request.UserId,
            BusinessName = request.BusinessName,
            Description = request.Description,
            IsActive = true,
            ActivatedAt = DateTime.UtcNow
        };

        await _vendorRepository.AddAsync(vendorProfile, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new VendorProfileDto
        {
            Id = vendorProfile.Id,
            BusinessName = vendorProfile.BusinessName,
            Description = vendorProfile.Description,
            IsActive = vendorProfile.IsActive,
            ActivatedAt = vendorProfile.ActivatedAt
        };
    }
}

