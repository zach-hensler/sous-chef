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
        Assert.NotNull(storedRes.Data);
        
        Assert.Equal(requested.Recipe.Name, storedRes.Data.Name);
        Assert.Equal(requested.Recipe.Description, storedRes.Data.Description);
        Assert.Equal(requested.Ingredients.Count, storedRes.Data.Ingredients.Count);
        foreach (var ingredient in requested.Ingredients) {
            Assert.NotNull(storedRes.Data.Ingredients.Find(stored =>
                stored.Name == ingredient.Name &&
                Math.Abs(stored.Quantity - ingredient.Quantity) < .1f &&
                stored.Unit == ingredient.Unit));
        }

        var stepCounter = 0;
        foreach (var step in requested.Steps) {
            Assert.Equal(step.Name, storedRes.Data.Steps[stepCounter].Name);
            Assert.Equal(step.Instruction, storedRes.Data.Steps[stepCounter].Instruction);
            stepCounter++;
        }
    }
    
    [Fact]
    public async Task ShouldUpdateRecipe() {
        var conn = await Setup.GetConnectionFactory();
        var recipeService = new RecipeService(conn);

        var createRes = await recipeService.CreateRecipe(Rand.Domain.CreateRecipeRequest());
        Assert.Empty(createRes.ErrorMessage);
        
        var updated = Rand.Domain.CreateRecipeRequest();
        await recipeService.UpdateRecipe(createRes.Data, updated);
        var storedRes = await recipeService.GetRecipe(createRes.Data);
        Assert.Empty(storedRes.ErrorMessage);
        Assert.NotNull(storedRes.Data);
        
        Assert.Equal(updated.Recipe.Name, storedRes.Data.Name);
        Assert.Equal(updated.Recipe.Description, storedRes.Data.Description);
        Assert.Equal(updated.Ingredients.Count, storedRes.Data.Ingredients.Count);
        foreach (var ingredient in updated.Ingredients) {
            Assert.NotNull(storedRes.Data.Ingredients.Find(stored =>
                stored.Name == ingredient.Name &&
                Math.Abs(stored.Quantity - ingredient.Quantity) < .1f &&
                stored.Unit == ingredient.Unit));
        }

        var stepCounter = 0;
        foreach (var step in updated.Steps) {
            Assert.Equal(step.Name, storedRes.Data.Steps[stepCounter].Name);
            Assert.Equal(step.Instruction, storedRes.Data.Steps[stepCounter].Instruction);
            stepCounter++;
        }
    }
}