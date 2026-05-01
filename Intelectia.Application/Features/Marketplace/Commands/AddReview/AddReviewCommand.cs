using MediatR;
using Intelectia.Shared.DTOs.Marketplace;

namespace Intelectia.Application.Features.Marketplace.Commands.AddReview;

public record AddReviewCommand(
    Guid    BookId,
    Guid    UserId,
    int     Rating,
    string? Comment
) : IRequest<ReviewDto>;
