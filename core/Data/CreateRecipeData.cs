using core.Models;
using Dapper;

namespace core.Data;

public class CreateRecipeData(IConnectionFactory connectionFactory) {
    public async Task CreateRecipe(
        CreateRecipeDb recipe,
        List<CreateRecipeStepDb> steps,
        List<CreateRecipeIngredientDb> ingredients) {

        await using var conn = connectionFactory.GetConnection();
        var transaction = await conn.BeginTransactionAsync();
        var recipeId = await Common.Recipe.Create(recipe, conn);

        var version = await Common.RecipeVersion.Create(new CreateRecipeVersionDb {
            RecipeId = recipeId,
            VersionNumber = "1.0",
            CreatedAt = DateTime.UtcNow
        }, conn);
        
        for (var i = 1; i <= steps.Count; i++) {
            await Common.RecipeSteps.Create(steps[i], i, version, conn);
        }

        foreach (var ingredient in ingredients) {
            await Common.RecipeIngredients.Create(ingredient, version, conn);
        }

        await transaction.CommitAsync();
    }
}