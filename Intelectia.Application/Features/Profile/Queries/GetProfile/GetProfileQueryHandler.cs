using MediatR;
using Intelectia.Application.Common.Exceptions;
using Intelectia.Domain.Entities;
using Intelectia.Domain.Interfaces.Repositories;
using Intelectia.Shared.DTOs.Profile;

namespace Intelectia.Application.Features.Profile.Queries.GetProfile;

public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, UserProfileDto>
{
    private readonly IUserRepository _userRepository;

    public GetProfileQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserProfileDto> Handle(
        GetProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdWithProfilesAsync(
            request.UserId, cancellationToken);

        if (user is null)
            throw new NotFoundException(nameof(User), request.UserId);

        return new UserProfileDto
        {
            Id                = user.Id,
            Email             = user.Email,
            FirstName         = user.FirstName,
            LastName          = user.LastName,
            ProfilePictureUrl = user.ProfilePictureUrl,
            IsStudent         = user.StudentProfile is not null,
            IsVendor          = user.VendorProfile is not null && user.VendorProfile.IsActive,
            LastLoginAt       = user.LastLoginAt
        };
    }
}
