using core.Data;
using core.Models;
using core.Models.DbModels;

namespace services;

public static class RecipeService {
    public static async Task<RecipeVersionDb?> CreateRecipe(CreateRecipeRequest request) {
        return await Utils.SafeRun(nameof(CreateRecipe), async conn => {
            var transaction = await conn.BeginTransactionAsync();
            var recipeId = await Common.Recipe.Create(request.Recipe, conn);

            var now = DateTime.UtcNow;
            var versionId = await Common.Version.Create(new CreateRecipeVersionDb {
                Message = "Initial",
                RecipeId = recipeId,
                CreatedAt = now
            }, conn);

            for (var i = 0; i < request.Steps.Count; i++) {
                await Common.RecipeSteps.Create(request.Steps[i], i + 1, versionId, conn);
            }

            foreach (var ingredient in request.Ingredients) {
                await Common.RecipeIngredients.Create(ingredient, versionId, conn);
            }

            await transaction.CommitAsync();
            return new RecipeVersionDb {
                VersionId = versionId,
                RecipeId = recipeId,
                CreatedAt = now,
                Message = "Initial"
            };
        });
    }

    public static async Task<VersionId?> CreateRecipeVersion(CreateRecipeVersionRequest request) {
        return await Utils.SafeRun(nameof(CreateRecipe), async conn => {
            var transaction = await conn.BeginTransactionAsync();

            var recipeId = (await Common.Version.Get(request.PreviousVersionId, conn)).RecipeId;
            await Common.Recipe.Update(recipeId, request.Recipe, conn);

            var version = await Common.Version.Create(new CreateRecipeVersionDb {
                Message = request.Message,
                RecipeId = recipeId,
                CreatedAt = DateTime.UtcNow
            }, conn);

            for (var i = 0; i < request.Steps.Count; i++) {
                await Common.RecipeSteps.Create(request.Steps[i], i + 1, version, conn);
            }

            foreach (var ingredient in request.Ingredients) {
                await Common.RecipeIngredients.Create(ingredient, version, conn);
            }

            await transaction.CommitAsync();
            return version;
        });
    }

    public static async Task<VersionId?> UpdateRecipeVersion(VersionId versionId, CreateRecipeRequest request) {
        return await Utils.SafeRun(nameof(UpdateRecipeVersion), async conn => {
            var transaction = await conn.BeginTransactionAsync();

            var version = await Common.Version.Get(versionId, conn);
            await Common.Recipe.Update(version.RecipeId, request.Recipe, conn);

            await Common.RecipeSteps.Delete(versionId, conn);
            for (var i = 0; i < request.Steps.Count; i++) {
                await Common.RecipeSteps.Create(request.Steps[i], i + 1, versionId, conn);
            }

            await Common.RecipeIngredients.Delete(versionId, conn);
            foreach (var ingredient in request.Ingredients) {
                await Common.RecipeIngredients.Create(ingredient, versionId, conn);
            }

            await transaction.CommitAsync();
            return version.VersionId;
        });
    }

    public static async Task<RecipeDetails?> GetRecipe(RecipeId recipeId) {
        return await Utils.SafeRun(nameof(GetRecipe),
            async conn => await GetRecipeDetails.GetByRecipe(recipeId, conn));
    }

    public static async Task<RecipeDetails?> GetRecipeByVersion(VersionId versionId) {
        return await Utils.SafeRun(nameof(GetRecipeByVersion),
            async conn => await GetRecipeDetails.GetByVersion(versionId, conn));
    }

    public static async Task<ListRecipesResponse?> ListRecipes(ListRecipesRequest request) {
        return await Utils.SafeRun(nameof(ListRecipes), async conn => {
            var total = await Common.Recipe.Count(conn);
            var recipes = await core.Data.ListRecipes.Get(request.Count, request.Offset, conn);

            return new ListRecipesResponse {
                Total = total,
                Items = recipes
            };
        });
    }

    public static async Task<List<RecipeCommentDb>> GetComments(RecipeId recipeId) {
        return
            await Utils.SafeRun(nameof(GetComments), async conn => {
                if (!await Common.Recipe.Exists(recipeId, conn)) {
                    throw new Exception($"Version '{recipeId}' not found");
                }

                return await Common.RecipeComments.GetByRecipe(recipeId, conn);
            })
            ?? [];
    }
}