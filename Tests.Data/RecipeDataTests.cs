using core.Data;
using Helpers;
using services;
using Xunit;

namespace DataTests;

public class RecipeDataTests: Sequential {
    [Fact]
    public async Task ShouldDeleteRecipe() {
        await using var conn = await Setup.ResetAndGetDatabase();

        var created1 = await RecipeService.CreateRecipe(Rand.Domain.Requests.CreateRecipeRequest());
        var created2 = await RecipeService.CreateRecipe(Rand.Domain.Requests.CreateRecipeRequest());
        await Common.Recipe.DeleteCascade(created1!.RecipeId, conn);
        
        Assert.False(await Common.Recipe.Exists(created1.RecipeId, conn));
        Assert.True(await Common.Recipe.Exists(created2!.RecipeId, conn));
    }

    [Fact]
    public async Task ShouldDeleteRecipeFromVersion() {
        await using var conn = await Setup.ResetAndGetDatabase();

        var created1 = await RecipeService.CreateRecipe(Rand.Domain.Requests.CreateRecipeRequest());
        var created2 = await RecipeService.CreateRecipe(Rand.Domain.Requests.CreateRecipeRequest());
        await Common.Recipe.DeleteCascadeFromVersion(created1!.VersionId, conn);
        
        Assert.False(await Common.Recipe.Exists(created1.RecipeId, conn));
        Assert.True(await Common.Recipe.Exists(created2!.RecipeId, conn));
    }
}