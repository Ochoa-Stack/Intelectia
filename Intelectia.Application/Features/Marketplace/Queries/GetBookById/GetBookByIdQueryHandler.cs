using AutoMapper;
using MediatR;
using Intelectia.Application.Common.Exceptions;
using Intelectia.Domain.Entities;
using Intelectia.Domain.Interfaces.Repositories;
using Intelectia.Shared.DTOs.Marketplace;

namespace Intelectia.Application.Features.Marketplace.Queries.GetBookById;

public class GetBookByIdQueryHandler : IRequestHandler<GetBookByIdQuery, BookDetailDto>
{
    private readonly IBookRepository _bookRepository;
    private readonly IMapper _mapper;

    public GetBookByIdQueryHandler(IBookRepository bookRepository, IMapper mapper)
    {
        _bookRepository = bookRepository;
        _mapper = mapper;
    }

    public async Task<BookDetailDto> Handle(
        GetBookByIdQuery request, CancellationToken cancellationToken)
    {
        // Buscamos el libro con todos sus detalles para la vista de detalle
        var book = await _bookRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken);

        if (book is null)
            throw new NotFoundException(nameof(Book), request.Id);

        return _mapper.Map<BookDetailDto>(book);
    }
}
