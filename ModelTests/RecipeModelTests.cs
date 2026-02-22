using core.Data;
using core.Models;
using Helpers;
using services;
using sous_chef.Pages;
using Xunit;

namespace PageTests;

public class RecipeModelTests: Sequential {
    [Fact]
    public async Task ShouldGetPageWithOneVersion() {
        await using var conn = (await Setup.ResetAndGetDatabase()).GetConnection();
        await conn.OpenAsync(TestContext.Current.CancellationToken);

        var recipe = await RecipeService.CreateRecipe(Rand.Domain.Requests.CreateRecipeRequest());
        Assert.Empty(recipe.ErrorMessage);
        
        var latest = await Common.RecipeVersion.GetLatest(recipe.Data!.RecipeId, conn);
        var model = new RecipeModel();

        await model.OnGet(latest.VersionId);
        Assert.NotNull(model.Details);
        Assert.Single(model.Versions);
    }

    [Fact]
    public async Task ShouldGetPageWhenMultipleVersionsExist() {
        await using var conn = (await Setup.ResetAndGetDatabase()).GetConnection();
        await conn.OpenAsync(TestContext.Current.CancellationToken);

        var originalRecipe = Rand.Domain.Requests.CreateRecipeRequest();
        var recipe = await RecipeService.CreateRecipe(originalRecipe);
        Assert.Empty(recipe.ErrorMessage);
        
        const int versionId1 = 1; //we know this because fresh db
        var versionId2 = await RecipeService.CreateRecipeVersion(new CreateRecipeVersionRequest {
            PreviousVersionId = versionId1,
            VersionType = VersionType.Minor,
            Recipe = originalRecipe.Recipe,
            Steps = originalRecipe.Steps,
            Ingredients = originalRecipe.Ingredients
        });
        Assert.Empty(versionId2.ErrorMessage);
        
        var versionId3 = await RecipeService.CreateRecipeVersion(new CreateRecipeVersionRequest {
            PreviousVersionId = versionId1,
            VersionType = VersionType.Minor,
            Recipe = originalRecipe.Recipe,
            Steps = originalRecipe.Steps,
            Ingredients = originalRecipe.Ingredients
        });
        Assert.Empty(versionId3.ErrorMessage);
        
        var versionId4 = await RecipeService.CreateRecipeVersion(new CreateRecipeVersionRequest {
            PreviousVersionId = versionId1,
            VersionType = VersionType.Major,
            Recipe = originalRecipe.Recipe,
            Steps = originalRecipe.Steps,
            Ingredients = originalRecipe.Ingredients
        });
        Assert.Empty(versionId4.ErrorMessage);

        var latest = await Common.RecipeVersion.GetLatest(recipe.Data!.RecipeId, conn);

        var model = new RecipeModel();
        await model.OnGet(versionId3.Data);
        Assert.NotNull(model.Details);
        Assert.Equal("1.2", model.Details.Version.VersionNumber);
        Assert.NotEqual(model.Details.Version.VersionNumber, latest.VersionNumber);
        Assert.Equal(4, model.Versions.Count);
    }
}