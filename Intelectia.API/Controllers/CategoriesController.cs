using MediatR;
using Microsoft.AspNetCore.Mvc;
using Intelectia.Application.Features.Marketplace.Queries.GetCategories;

namespace Intelectia.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary> Devuelve todas las categorías activas para los filtros del catálogo. </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCategoriesQuery(), cancellationToken);
        return Ok(result);
    }
}
