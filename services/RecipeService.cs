using System.Net;
using core;
using core.Data;
using core.Models;

namespace services;

public class RecipeService(IConnectionFactory connectionFactory) {
    
    public async Task<Response> CreateRecipe(CreateRecipeRequest request) {
        try {
            await new CreateRecipeData(connectionFactory).CreateRecipe(request);

            return new Response();
        }
        catch (Exception ex) {
            return new Response(HttpStatusCode.InternalServerError, ex.Message);
        }
    }
}