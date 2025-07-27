namespace NicolasQuiPaieAPI.Presentation.Endpoints;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories")
            .WithTags("Categories")
            .WithOpenApi()
            .RequireUserRole();

        // GET /api/categories
        group.MapGet("/", async (
            [FromServices] ICategoryRepository categoryRepository,
            [FromServices] ILogger<Program> logger) =>
        {
            try
            {
                var categories = await categoryRepository.GetAllAsync();
                var categoriesList = categories.ToList();

                if (categoriesList.Count == 0)
                {
                    logger.LogWarning("No categories found in the system");
                }

                var categoryDtos = categoriesList.Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Color = c.Color,
                    IconClass = c.IconClass,
                    IsActive = c.IsActive,
                    ProposalsCount = c.Proposals?.Count ?? 0
                }).ToList();

                return Results.Ok(categoryDtos);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving categories");
                return Results.Problem("Error retrieving categories");
            }
        })
        .WithName("GetCategories")
        .WithSummary("Récupère toutes les catégories")
        .Produces<IEnumerable<CategoryDto>>()
        .Produces(500);

        // GET /api/categories/{id}
        group.MapGet("/{id:int}", async (
            [FromServices] ICategoryRepository categoryRepository,
            [FromServices] ILogger<Program> logger,
            int id) =>
        {
            try
            {
                if (id <= 0)
                {
                    logger.LogWarning("Invalid category ID: {CategoryId}. Must be > 0", id);
                    return Results.BadRequest("Invalid category ID");
                }

                var category = await categoryRepository.GetByIdAsync(id);
                if (category == null)
                {
                    logger.LogWarning("Category not found: {CategoryId}", id);
                    return Results.NotFound();
                }

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
                logger.LogError(ex, "Error retrieving category: {CategoryId}", id);
                return Results.Problem("Error retrieving category");
            }
        })
        .WithName("GetCategoryById")
        .WithSummary("Récupère une catégorie par son ID")
        .Produces<CategoryDto>()
        .Produces(400)
        .Produces(404)
        .Produces(500);

        // GET /api/categories/active
        group.MapGet("/active", async (
            [FromServices] ICategoryRepository categoryRepository,
            [FromServices] ILogger<Program> logger) =>
        {
            try
            {
                var categories = await categoryRepository.GetActiveCategoriesAsync();
                var categoriesList = categories.ToList();

                if (categoriesList.Count == 0)
                {
                    logger.LogWarning("No active categories found in the system");
                }

                var categoryDtos = categoriesList.Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Color = c.Color,
                    IconClass = c.IconClass,
                    IsActive = c.IsActive,
                    ProposalsCount = c.Proposals?.Count ?? 0
                }).ToList();

                return Results.Ok(categoryDtos);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving active categories");
                return Results.Problem("Error retrieving active categories");
            }
        })
        .WithName("GetActiveCategories")
        .WithSummary("Récupère toutes les catégories actives")
        .Produces<IEnumerable<CategoryDto>>()
        .Produces(500);
    }
}