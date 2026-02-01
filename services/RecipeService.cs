using System.Net;
using core;
using core.Data;
using core.Models;

namespace services;


public class RecipeService(IConnectionFactory connectionFactory) {
    
    public async Task<Response> CreateRecipe(CreateRecipeRequest request) {
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
            await conn.CloseAsync();;

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
}
