using AutoMapper;
using MediatR;
using Intelectia.Domain.Interfaces.Repositories;
using Intelectia.Shared.DTOs.Library;

namespace Intelectia.Application.Features.Library.Queries.GetNotes;

public class GetNotesQueryHandler : IRequestHandler<GetNotesQuery, IReadOnlyList<NoteDto>>
{
    private readonly INoteRepository _noteRepository;
    private readonly IMapper _mapper;

    public GetNotesQueryHandler(INoteRepository noteRepository, IMapper mapper)
    {
        _noteRepository = noteRepository;
        _mapper         = mapper;
    }

    public async Task<IReadOnlyList<NoteDto>> Handle(
        GetNotesQuery request, CancellationToken cancellationToken)
    {
        var notes = await _noteRepository.GetByUserIdAsync(
            request.UserId, request.BookId, cancellationToken);
        return _mapper.Map<IReadOnlyList<NoteDto>>(notes);
    }
}
