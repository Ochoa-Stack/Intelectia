using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Intelectia.Application.Common.Interfaces;
using Intelectia.Shared.DTOs.Library;

namespace Intelectia.Application.Features.Library.Queries.GetUserBooks;

public class GetUserBooksQueryHandler : IRequestHandler<GetUserBooksQuery, IReadOnlyList<UserBookDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetUserBooksQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper  = mapper;
    }

    public async Task<IReadOnlyList<UserBookDto>> Handle(
        GetUserBooksQuery request, CancellationToken cancellationToken)
    {
        // Cargamos los UserBooks con Book -> Category para que el mapper pueda acceder a CategoryName
        var userBooks = await _context.UserBooks
            .Include(ub => ub.Book)
                .ThenInclude(b => b.Category)
            .Where(ub => ub.UserId == request.UserId && !ub.IsDeleted)
            .OrderByDescending(ub => ub.AcquiredAt)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IReadOnlyList<UserBookDto>>(userBooks);
    }
}
