using AutoMapper;
using MediatR;
using Intelectia.Application.Common.Exceptions;
using Intelectia.Application.Common.Interfaces;
using Intelectia.Domain.Entities;
using Intelectia.Domain.Interfaces;
using Intelectia.Domain.Interfaces.Repositories;
using Intelectia.Shared.DTOs.Marketplace;

namespace Intelectia.Application.Features.Marketplace.Commands.AddReview;

public class AddReviewCommandHandler : IRequestHandler<AddReviewCommand, ReviewDto>
{
    private readonly IBookRepository _bookRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly IRepository<UserBook> _userBookRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AddReviewCommandHandler(
        IBookRepository bookRepository,
        IReviewRepository reviewRepository,
        IRepository<UserBook> userBookRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _bookRepository = bookRepository;
        _reviewRepository = reviewRepository;
        _userBookRepository = userBookRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ReviewDto> Handle(AddReviewCommand request, CancellationToken cancellationToken)
    {
        // Verificamos que el libro exista
        var book = await _bookRepository.GetByIdWithDetailsAsync(request.BookId, cancellationToken);
        if (book is null)
            throw new NotFoundException(nameof(Book), request.BookId);

        // Un usuario no puede reseñar el mismo libro dos veces
        var existingReview = await _reviewRepository.GetReviewByUserAndBookAsync(request.UserId, request.BookId, cancellationToken);

        if (existingReview is not null)
            throw new ConflictException("Ya escribiste una reseña para este libro.");

        // Solo se puede reseñar un libro que se haya adquirido
        var hasPurchased = await _userBookRepository.AnyAsync(ub =>
            ub.BookId == request.BookId &&
            ub.UserId == request.UserId,
            cancellationToken);

        if (!hasPurchased)
            throw new ConflictException("Solo puedes reseñar libros que hayas adquirido.");

        // Insertamos la reseña directamente en el DbSet para garantizar estado Added
        var review = new Review
        {
            BookId  = request.BookId,
            UserId  = request.UserId,
            Rating  = request.Rating,
            Comment = request.Comment
        };

        await _reviewRepository.AddAsync(review, cancellationToken);

        // Recalculamos el promedio y el contador del libro
        book.ReviewCount   = book.Reviews.Count + 1;
        book.AverageRating = (book.Reviews.Sum(r => r.Rating) + request.Rating)
                             / (double)book.ReviewCount;

        _bookRepository.Update(book);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Recargamos la reseña con el usuario para construir el DTO de respuesta
        var reviewWithUser = await _reviewRepository.GetByIdWithUserAsync(review.Id, cancellationToken)
                             ?? throw new NotFoundException(nameof(Review), review.Id);

        return _mapper.Map<ReviewDto>(reviewWithUser);
    }
}

