using Microsoft.AspNetCore.Mvc;
using NicolasQuiPaieAPI.Application.Interfaces;
using NicolasQuiPaieData.DTOs;

namespace NicolasQuiPaieAPI.Presentation.Endpoints
{
    public static class CategoryEndpoints
    {
        public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/categories")
                .WithTags("Categories")
                .WithOpenApi();

            // GET /api/categories
            group.MapGet("/", async ([FromServices] ICategoryRepository categoryRepository) =>
            {
                try
                {
                    var categories = await categoryRepository.GetAllAsync();
                    var categoryDtos = categories.Select(c => new CategoryDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        Color = c.Color,
                        IconClass = c.IconClass,
                        IsActive = c.IsActive,
                        ProposalsCount = c.Proposals?.Count ?? 0
                    });
                    return Results.Ok(categoryDtos);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error retrieving categories: {ex.Message}");
                }
            })
            .WithName("GetCategories")
            .WithSummary("Récupère toutes les catégories")
            .Produces<IEnumerable<CategoryDto>>();

            // GET /api/categories/{id}
            group.MapGet("/{id:int}", async (
                [FromServices] ICategoryRepository categoryRepository,
                int id) =>
            {
                try
                {
                    var category = await categoryRepository.GetByIdAsync(id);
                    if (category == null)
                        return Results.NotFound();

                    var categoryDto = new CategoryDto
                    {
                        Id = category.Id,
                        Name = category.Name,
                        Description = category.Description,
                        Color = category.Color,
                        IconClass = category.IconClass,
                        IsActive = category.IsActive,
                        ProposalsCount = category.Proposals?.Count ?? 0
                    };
                    return Results.Ok(categoryDto);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error retrieving category: {ex.Message}");
                }
            })
            .WithName("GetCategoryById")
            .WithSummary("Récupère une catégorie par son ID")
            .Produces<CategoryDto>()
            .Produces(404);
        }
    }
}