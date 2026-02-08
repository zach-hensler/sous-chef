using System.Net;
using core;
using core.Data;
using core.Models;

namespace services;

public class RecipeService(IConnectionFactory connectionFactory) {
    public async Task<Response<int>> CreateRecipe(CreateRecipeRequest request) {
        try {
            await using var conn = connectionFactory.GetConnection();
            await conn.OpenAsync();
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
            await conn.CloseAsync();
            ;

            return new Response<int>(recipeId);
        }
        catch (Exception ex) {
            return new Response<int>(HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    public async Task<Response> UpdateRecipe(int versionId, CreateRecipeRequest request) {
        try {
            await using var conn = connectionFactory.GetConnection();
            await conn.OpenAsync();
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
            await conn.CloseAsync();
            ;

            return new Response();
        }
        catch (Exception ex) {
            return new Response(HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    public async Task<Response<RecipeDetails>> GetRecipe(int recipeId) {
        try {
            await using var conn = connectionFactory.GetConnection();
            await conn.OpenAsync();

            var data = await GetRecipeDetails.Get(recipeId, conn);

            return new Response<RecipeDetails>(data);
        }
        catch (Exception ex) {
            return new Response<RecipeDetails>(HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    public async Task<Response> DeleteRecipe(int recipeId) {
        try {
            await using var conn = connectionFactory.GetConnection();
            await conn.OpenAsync();

            await Common.Recipe.DeleteCascade(recipeId, conn);

            return new Response();
        }
        catch (Exception ex) {
            return new Response(HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    public async Task<Response<ListRecipesResponse>> ListRecipes(ListRecipesRequest request) {
        try {
            await using var conn = connectionFactory.GetConnection();
            await conn.OpenAsync();

            var total = await RecipeData.CountRecipes(conn);
            var recipes = await RecipeData.ListRecipes(request.Count, request.Offset, conn);

            return new Response<ListRecipesResponse>(new ListRecipesResponse {
                Total = total,
                Items =
                    recipes.Select(r => new ListRecipesResponse.RecipeItem {
                            Id = r.recipe_id,
                            Name = r.name,
                            Description = r.description,
                            EffortLevel = r.effort_level
                        })
                        .ToList()
            });
        }
        catch (Exception ex) {
            return new Response<ListRecipesResponse>(HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    public async Task<Response> AddComment(CreateRecipeCommentDb request) {
        try {
            await using var conn = connectionFactory.GetConnection();
            await conn.OpenAsync();

            if (!await Common.RecipeVersion.Exists(request.VersionId, conn)) {
                return new Response(HttpStatusCode.NotFound, $"Version '{request.VersionId}' not found");
            }

            await Common.RecipeComments.Create(request, conn);

            return new Response();
        }
        catch (Exception ex) {
            return new Response(HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    public async Task<Response<List<GetCommentsResponse>>> GetComments(int versionId) {
        try {
            await using var conn = connectionFactory.GetConnection();
            await conn.OpenAsync();

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
        }
        catch (Exception ex) {
            return new Response<List<GetCommentsResponse>>(HttpStatusCode.InternalServerError, ex.Message);
        }
    }
}