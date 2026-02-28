using System.Net;
using core.Data;
using core.Models;
using core.Models.DbModels;

namespace services;

public static class RecipeService {
    public static async Task<Response<RecipeVersionDb>> CreateRecipe(CreateRecipeRequest request) {
        return await Utils.SafeRun(nameof(CreateRecipe), async (conn) => {
            var transaction = await conn.BeginTransactionAsync();
            var recipeId = await Common.Recipe.Create(request.Recipe, conn);

            var now = DateTime.UtcNow;
            var versionId = await Common.RecipeVersion.Create(new CreateRecipeVersionDb {
                RecipeId = recipeId,
                VersionNumber = "1.0",
                CreatedAt = now
            }, conn);

            for (var i = 0; i < request.Steps.Count; i++) {
                await Common.RecipeSteps.Create(request.Steps[i], i + 1, versionId, conn);
            }

            foreach (var ingredient in request.Ingredients) {
                await Common.RecipeIngredients.Create(ingredient, versionId, conn);
            }

            await transaction.CommitAsync();
            return new Response<RecipeVersionDb>(new RecipeVersionDb {
                VersionId = versionId,
                VersionNumber = "1.0",
                RecipeId = recipeId,
                CreatedAt = default
            });
        });
    }

    public static async Task<Response<VersionId>> CreateRecipeVersion(CreateRecipeVersionRequest request) {
        return await Utils.SafeRun(nameof(CreateRecipe), async (conn) => {
            var transaction = await conn.BeginTransactionAsync();
            
            var recipeId = (await Common.RecipeVersion.Get(request.PreviousVersionId, conn)).RecipeId;
            await Common.Recipe.Update(recipeId, request.Recipe, conn);
            var latestVersion = await Common.RecipeVersion.GetLatest(recipeId, conn);

            var version = await Common.RecipeVersion.Create(new CreateRecipeVersionDb {
                RecipeId = recipeId,
                VersionNumber = Utils.Domain.IncrementRecipeVersion(latestVersion.VersionNumber, request.VersionType),
                CreatedAt = DateTime.UtcNow
            }, conn);

            for (var i = 0; i < request.Steps.Count; i++) {
                await Common.RecipeSteps.Create(request.Steps[i], i + 1, version, conn);
            }

            foreach (var ingredient in request.Ingredients) {
                await Common.RecipeIngredients.Create(ingredient, version, conn);
            }

            await transaction.CommitAsync();
            return new Response<VersionId>(version);
        });
        
    }

    public static async Task<Response> UpdateRecipeVersion(VersionId versionId, CreateRecipeRequest request) {
        return await Utils.SafeRun(nameof(UpdateRecipeVersion), async (conn) => {
            var transaction = await conn.BeginTransactionAsync();

            var version = await Common.RecipeVersion.Get(versionId, conn);
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
            return new Response();
        });
    }

    public static async Task<Response<RecipeDetails>> GetRecipe(RecipeId recipeId) {
        return await Utils.SafeRun(nameof(GetRecipe), async (conn) => {
            var data = await GetRecipeDetails.GetByRecipe(recipeId, conn);

            return new Response<RecipeDetails>(data);
        });
    }

    public static async Task<Response<RecipeDetails>> GetRecipeByVersion(VersionId versionId) {
        return await Utils.SafeRun(nameof(GetRecipeByVersion), async (conn) => {
            var version = await Common.RecipeVersion.Get(versionId, conn);
            var data = await GetRecipeDetails.GetByRecipe(version.RecipeId, conn);

            return new Response<RecipeDetails>(data);
        });
    }
    public static async Task<Response<ListRecipesResponse>> ListRecipes(ListRecipesRequest request) {
        return await Utils.SafeRun(nameof(ListRecipes), async (conn) => {
            var total = await Common.Recipe.Count(conn);
            var recipes = await RecipeData.ListRecipes(request.Count, request.Offset, conn);

            return new Response<ListRecipesResponse>(new ListRecipesResponse {
                Total = total,
                Items = recipes
            });
        });
    }

    public static async Task<Response<List<RecipeCommentDb>>> GetComments(RecipeId recipeId) {
        return await Utils.SafeRun(nameof(GetComments), async (conn) => {
            if (!await Common.Recipe.Exists(recipeId, conn)) {
                return new Response<List<RecipeCommentDb>>(
                    HttpStatusCode.NotFound, $"Version '{recipeId}' not found");
            }

            return new Response<List<RecipeCommentDb>>(
                await Common.RecipeComments.GetByRecipe(recipeId, conn));
        });
    }
}