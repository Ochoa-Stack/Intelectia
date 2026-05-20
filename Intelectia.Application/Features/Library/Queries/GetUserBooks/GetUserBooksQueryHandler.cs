using AutoMapper;
using MediatR;
using Intelectia.Domain.Interfaces.Repositories;
using Intelectia.Shared.DTOs.Library;

namespace Intelectia.Application.Features.Library.Queries.GetUserBooks;

public class GetUserBooksQueryHandler : IRequestHandler<GetUserBooksQuery, IReadOnlyList<UserBookDto>>
{
    private readonly IUserBookRepository _userBookRepository;
    private readonly IMapper _mapper;

    public GetUserBooksQueryHandler(IUserBookRepository userBookRepository, IMapper mapper)
    {
        _userBookRepository = userBookRepository;
        _mapper  = mapper;
    }

    public async Task<IReadOnlyList<UserBookDto>> Handle(
        GetUserBooksQuery request, CancellationToken cancellationToken)
    {
        // Cargamos los UserBooks con detalles abstraídos en el repositorio
        var userBooks = await _userBookRepository.GetUserBooksWithDetailsAsync(request.UserId, cancellationToken);

        return _mapper.Map<IReadOnlyList<UserBookDto>>(userBooks);
    }
}
