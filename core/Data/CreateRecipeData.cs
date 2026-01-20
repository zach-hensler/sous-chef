using core.Models;

namespace core.Data;

public record CreateRecipeRequest {
    public required CreateRecipeDb Recipe { get; init; }
    public required List<CreateRecipeStepDb> Steps { get; init; }
    public required List<CreateRecipeIngredientDb> Ingredients { get; init; }
}

public class CreateRecipeData(IConnectionFactory connectionFactory) {
    public async Task CreateRecipe(CreateRecipeRequest request) {

        await using var conn = connectionFactory.GetConnection();
        await conn.OpenAsync();
        var transaction = await conn.BeginTransactionAsync();
        var recipeId = await Common.Recipe.Create(request.Recipe, conn);

        var version = await Common.RecipeVersion.Create(new CreateRecipeVersionDb {
            RecipeId = recipeId,
            VersionNumber = "1.0",
            CreatedAt = DateTime.UtcNow
        }, conn);
        
        for (var i = 1; i <= request.Steps.Count; i++) {
            await Common.RecipeSteps.Create(request.Steps[i], i, version, conn);
        }

        foreach (var ingredient in request.Ingredients) {
            await Common.RecipeIngredients.Create(ingredient, version, conn);
        }

        await transaction.CommitAsync();
        await conn.CloseAsync();
    }
}