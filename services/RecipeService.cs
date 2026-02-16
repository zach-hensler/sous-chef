using System.Net;
using core.Data;
using core.Models;

namespace services;

public static class RecipeService {
    public static async Task<Response<int>> CreateRecipe(CreateRecipeRequest request) {
        return await Utils.SafeRun(nameof(CreateRecipe), async (conn) => {
            var transaction = await conn.BeginTransactionAsync();
            var recipeId = await Common.Recipe.Create(request.Recipe, conn);

            var version = await Common.RecipeVersion.Create(new CreateRecipeVersionDb {
                RecipeId = recipeId,
                VersionNumber = "1.0",
                CreatedAt = DateTime.UtcNow
            }, conn);

            for (var i = 0; i < request.Steps.Count; i++) {
                await Common.RecipeSteps.Create(request.Steps[i], i + 1, version, conn);
            }

            foreach (var ingredient in request.Ingredients) {
                await Common.RecipeIngredients.Create(ingredient, version, conn);
            }

            await transaction.CommitAsync();
            return new Response<int>(recipeId);
        });
    }

    public static async Task<Response> UpdateRecipe(int versionId, CreateRecipeRequest request) {
        return await Utils.SafeRun(nameof(UpdateRecipe), async (conn) => {
            var transaction = await conn.BeginTransactionAsync();

            var version = await Common.RecipeVersion.Get(versionId, conn);
            await Common.Recipe.Update(version.recipe_id, request.Recipe, conn);

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

    public static async Task<Response<RecipeDetails>> GetRecipe(int recipeId) {
        return await Utils.SafeRun(nameof(GetRecipe), async (conn) => {
            var data = await GetRecipeDetails.Get(recipeId, conn);

            return new Response<RecipeDetails>(data);
        });
    }

    public static async Task<Response> DeleteRecipe(int recipeId) {
        return await Utils.SafeRun(nameof(DeleteRecipe), async (conn) => {
            await Common.Recipe.DeleteCascade(recipeId, conn);

            return new Response();
        });
    }

    public static async Task<Response<ListRecipesResponse>> ListRecipes(ListRecipesRequest request) {
        return await Utils.SafeRun(nameof(ListRecipes), async (conn) => {
            var total = await RecipeData.CountRecipes(conn);
            var recipes = await RecipeData.ListRecipes(request.Count, request.Offset, conn);

            return new Response<ListRecipesResponse>(new ListRecipesResponse {
                Total = total,
                Items =
                    recipes.Select(r => new ListRecipesResponse.RecipeItem {
                            Id = r.recipe_id,
                            Name = r.name,
                            Description = r.description,
                            EffortLevel = r.effort_level,
                            Category = r.category
                        })
                        .ToList()
            });
        });
    }

    public static async Task<Response> AddComment(CreateRecipeCommentDb request) {
        return await Utils.SafeRun(nameof(AddComment), async (conn) => {
            if (!await Common.RecipeVersion.Exists(request.VersionId, conn)) {
                return new Response(HttpStatusCode.NotFound, $"Version '{request.VersionId}' not found");
            }

            await Common.RecipeComments.Create(request, conn);

            return new Response();
        });
    }

    public static async Task<Response<List<GetCommentsResponse>>> GetComments(int versionId) {
        return await Utils.SafeRun(nameof(GetComments), async (conn) => {
            if (!await Common.RecipeVersion.Exists(versionId, conn)) {
                return new Response<List<GetCommentsResponse>>(
                    HttpStatusCode.NotFound, $"Version '{versionId}' not found");
            }

            var comments = await Common.RecipeComments.Get(versionId, conn);
            return new Response<List<GetCommentsResponse>>(
                comments.Select(c => new GetCommentsResponse {
                        CommentId = c.comment_id,
                        Comment = c.comment,
                        Rating = c.rating,
                        CreatedAt = c.created_at
                    })
                    .ToList());
        });
    }
}