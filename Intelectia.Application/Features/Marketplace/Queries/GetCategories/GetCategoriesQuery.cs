using MediatR;
using Intelectia.Shared.DTOs.Marketplace;

namespace Intelectia.Application.Features.Marketplace.Queries.GetCategories;

public record GetCategoriesQuery : IRequest<IReadOnlyList<CategoryDto>>;
