using AutoMapper;
using MediatR;
using Intelectia.Domain.Interfaces.Repositories;
using Intelectia.Shared.DTOs.Marketplace;

namespace Intelectia.Application.Features.Marketplace.Queries.GetBooks;

public class GetBooksQueryHandler : IRequestHandler<GetBooksQuery, PagedResult<BookSummaryDto>>
{
    private readonly IBookRepository _bookRepository;
    private readonly IMapper _mapper;

    public GetBooksQueryHandler(IBookRepository bookRepository, IMapper mapper)
    {
        _bookRepository = bookRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<BookSummaryDto>> Handle(
        GetBooksQuery request, CancellationToken cancellationToken)
    {
        // Obtenemos los libros paginados con los filtros aplicados
        var (items, totalCount) = await _bookRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.Search,
            request.CategoryId,
            request.Format,
            request.MinPrice,
            request.MaxPrice,
            request.SortBy,
            cancellationToken);

        // Convertimos las entidades a DTOs para la respuesta
        var dtos = _mapper.Map<IReadOnlyList<BookSummaryDto>>(items);

        return new PagedResult<BookSummaryDto>
        {
            Items      = dtos,
            Page       = request.Page,
            PageSize   = request.PageSize,
            TotalCount = totalCount
        };
    }
}
