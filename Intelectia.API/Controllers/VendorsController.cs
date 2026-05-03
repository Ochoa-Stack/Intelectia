using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Intelectia.Application.Features.Vendor.Commands.BecomeVendor;
using Intelectia.Application.Features.Vendor.Commands.PublishBook;
using Intelectia.Application.Features.Vendor.Queries.GetVendorBooks;
using Intelectia.Application.Features.Vendor.Queries.GetVendorStats;
using Intelectia.Application.Common.Interfaces;
using Intelectia.Domain.Enums;
using Intelectia.Shared.DTOs.Vendor;

namespace Intelectia.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VendorsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _context;

    public VendorsController(IMediator mediator, IApplicationDbContext context)
    {
        _mediator = mediator;
        _context  = context;
    }

    // Activa el perfil de vendedor para el usuario autenticado
    [HttpPost("me/become-vendor")]
    public async Task<IActionResult> BecomeVendor(
        [FromBody] BecomeVendorRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(new BecomeVendorCommand(
            userId, request.BusinessName, request.Description), cancellationToken);
        return Ok(result);
    }

    // Devuelve los libros publicados por el vendedor autenticado
    [HttpGet("me/books")]
    public async Task<IActionResult> GetMyBooks(CancellationToken cancellationToken)
    {
        var vendorProfileId = await GetVendorProfileIdAsync(cancellationToken);
        var result = await _mediator.Send(
            new GetVendorBooksQuery(vendorProfileId), cancellationToken);
        return Ok(result);
    }

    // Publica un libro nuevo en el catálogo
    [HttpPost("me/books")]
    public async Task<IActionResult> PublishBook(
        [FromBody] PublishBookRequest request,
        CancellationToken cancellationToken)
    {
        var vendorProfileId = await GetVendorProfileIdAsync(cancellationToken);

        if (!Enum.TryParse<BookFormat>(request.Format, ignoreCase: true, out var format))
            format = BookFormat.PDF;

        var result = await _mediator.Send(new PublishBookCommand(
            vendorProfileId,
            request.Title,
            request.Author,
            request.Description,
            request.ISBN,
            request.PublishedYear,
            request.PageCount,
            request.Language,
            request.Price,
            format,
            request.CategoryId), cancellationToken);

        return CreatedAtAction(nameof(GetMyBooks), result);
    }

    // Devuelve las estadísticas de ventas del vendedor
    [HttpGet("me/stats")]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
    {
        var vendorProfileId = await GetVendorProfileIdAsync(cancellationToken);
        var result = await _mediator.Send(
            new GetVendorStatsQuery(vendorProfileId), cancellationToken);
        return Ok(result);
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Token inválido.");
        return Guid.Parse(claim);
    }

    // Resuelve el VendorProfileId desde el UserId del token
    private async Task<Guid> GetVendorProfileIdAsync(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var vendorProfile = await _context.VendorProfiles
            .FirstOrDefaultAsync(
                vp => vp.UserId == userId && vp.IsActive && !vp.IsDeleted,
                cancellationToken);

        if (vendorProfile is null)
            throw new UnauthorizedAccessException("No tienes un perfil de vendedor activo.");

        return vendorProfile.Id;
    }
}
