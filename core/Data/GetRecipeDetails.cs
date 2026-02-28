using System.Data.Common;
using core.Models;
using core.Models.DbModels;

namespace core.Data;

public record RecipeDetails {
    public required RecipeDb RecipeMetadata { get; init; }
    public required RecipeVersionDb Version { get; init; }
    public required List<RecipeStepDb> Steps { get; init; }
    public required List<RecipeIngredientDb> Ingredients { get; init; }
}

public static class GetRecipeDetails {

    public static async Task<RecipeDetails> GetByRecipe(RecipeId recipeId, DbConnection conn) {
        var latestVersion = await Common.RecipeVersion.GetLatest(recipeId, conn);

        return await GetByVersion(latestVersion.VersionId, conn);
    }

    public static async Task<RecipeDetails> GetByVersion(VersionId versionId, DbConnection conn) {
        var version = await Common.RecipeVersion.Get(versionId, conn);
        var recipe = await Common.Recipe.Get(version.RecipeId, conn);
        
        var steps = await Common.RecipeSteps.Get(versionId, conn);
        var ingredients = await Common.RecipeIngredients.Get(versionId, conn);

        return new RecipeDetails {
            RecipeMetadata = recipe,
            Version = version,
            Steps = steps,
            Ingredients = ingredients
        };
    }
}