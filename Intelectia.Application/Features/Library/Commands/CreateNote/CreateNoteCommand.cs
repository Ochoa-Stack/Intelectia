using MediatR;
using Intelectia.Shared.DTOs.Library;

namespace Intelectia.Application.Features.Library.Commands.CreateNote;

public record CreateNoteCommand(
    Guid UserId,
    Guid? BookId,
    string Title,
    string Content,
    int? PageNumber,
    string? HighlightedText,
    string? HighlightColor
) : IRequest<NoteDto>;
