using MediatR;
using Intelectia.Shared.DTOs.Library;

namespace Intelectia.Application.Features.Library.Queries.GetNotes;

public record GetNotesQuery(Guid UserId, Guid? BookId = null) : IRequest<IReadOnlyList<NoteDto>>;
