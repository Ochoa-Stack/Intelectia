using MediatR;
using Intelectia.Domain.Enums;
using Intelectia.Shared.DTOs.Library;

namespace Intelectia.Application.Features.Library.Commands.GenerateCitation;

public record GenerateCitationCommand(
    Guid UserId,
    Guid BookId,
    CitationFormat Format,
    int? PageNumber
) : IRequest<CitationDto>;
