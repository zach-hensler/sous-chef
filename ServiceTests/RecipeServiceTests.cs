using Xunit;
using Helpers;
using services;

namespace ServiceTests;

public class RecipeServiceTests {
    [Fact]
    public async Task ShouldCreateNewRecipe() {
        var conn = await Setup.GetConnectionFactory();
        var recipeService = new RecipeService(conn);

        var requested = Rand.Domain.CreateRecipeRequest();
        var createRes = await recipeService.CreateRecipe(requested);
        Assert.Empty(createRes.ErrorMessage);

        var storedRes = await recipeService.GetRecipe(createRes.Data);
        Assert.Empty(storedRes.ErrorMessage);
        
        // TODO assert that requested == stored
    }
    
    [Fact]
    public async Task ShouldUpdateRecipe() {
        var conn = await Setup.GetConnectionFactory();
        // TODO test this path before trusting your recipes w/ it
    }
}