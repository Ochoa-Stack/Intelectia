using MediatR;
using Intelectia.Domain.Entities;
using Intelectia.Domain.Enums;
using Intelectia.Domain.Interfaces;
using Intelectia.Domain.Interfaces.Repositories;
using Intelectia.Shared.DTOs.Vendor;
using Intelectia.Application.Common.Exceptions;

namespace Intelectia.Application.Features.Vendor.Commands.PublishBook;

public class PublishBookCommandHandler : IRequestHandler<PublishBookCommand, VendorBookDto>
{
    private readonly IBookRepository _bookRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PublishBookCommandHandler(
        IBookRepository bookRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _bookRepository     = bookRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork         = unitOfWork;
    }

    public async Task<VendorBookDto> Handle(
        PublishBookCommand request, CancellationToken cancellationToken)
    {
        // Verificamos que la categoría exista
        var category = await _categoryRepository.GetByIdAsync(
            request.CategoryId, cancellationToken);

        if (category is null)
            throw new NotFoundException(nameof(Category), request.CategoryId);

        // Creamos el libro con estado Active; el vendedor lo publica directamente
        var book = new Book
        {
            Title           = request.Title,
            Author          = request.Author,
            Description     = request.Description,
            ISBN            = request.ISBN,
            PublishedYear   = request.PublishedYear,
            PageCount       = request.PageCount,
            Language        = request.Language,
            Price           = request.Price,
            Format          = request.Format,
            Status          = BookStatus.Active,
            CategoryId      = request.CategoryId,
            VendorProfileId = request.VendorProfileId
        };

        await _bookRepository.AddAsync(book, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new VendorBookDto
        {
            Id           = book.Id,
            Title        = book.Title,
            Author       = book.Author,
            CategoryName = category.Name,
            Price        = book.Price,
            Format       = book.Format.ToString(),
            Status       = book.Status.ToString(),
            CreatedAt    = book.CreatedAt
        };
    }
}
