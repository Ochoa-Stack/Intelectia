using MediatR;
using Intelectia.Shared.DTOs.Profile;

namespace Intelectia.Application.Features.Profile.Commands.UpdateProfile;

public record UpdateProfileCommand(
    Guid UserId,
    string FirstName,
    string LastName
) : IRequest<UserProfileDto>;
