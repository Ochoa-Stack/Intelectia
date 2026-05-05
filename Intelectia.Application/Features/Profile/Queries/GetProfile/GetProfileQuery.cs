using MediatR;
using Intelectia.Shared.DTOs.Profile;

namespace Intelectia.Application.Features.Profile.Queries.GetProfile;

public record GetProfileQuery(Guid UserId) : IRequest<UserProfileDto>;
