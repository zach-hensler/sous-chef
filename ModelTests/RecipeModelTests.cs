using core.Data;
using core.Models;
using core.Models.DbModels;
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
        
        var latest = await Common.Version.GetLatest(recipe.Data!.RecipeId, conn);
        var model = new RecipeModel();

        await model.OnGet(latest.VersionId.Value);
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
        
        var versionId1 = new VersionId(1); //we know this because fresh db
        var versionId2 = await RecipeService.CreateRecipeVersion(new CreateRecipeVersionRequest {
            PreviousVersionId = versionId1,
            VersionType = VersionType.Minor,
            Recipe = originalRecipe.Recipe,
            Steps = originalRecipe.Steps,
            Ingredients = originalRecipe.Ingredients,
            Message = Rand.Primitive.String()
        });
        Assert.Empty(versionId2.ErrorMessage);
        
        var versionId3 = await RecipeService.CreateRecipeVersion(new CreateRecipeVersionRequest {
            PreviousVersionId = versionId1,
            VersionType = VersionType.Minor,
            Recipe = originalRecipe.Recipe,
            Steps = originalRecipe.Steps,
            Ingredients = originalRecipe.Ingredients,
            Message = Rand.Primitive.String()
        });
        Assert.Empty(versionId3.ErrorMessage);
        
        var versionId4 = await RecipeService.CreateRecipeVersion(new CreateRecipeVersionRequest {
            PreviousVersionId = versionId1,
            VersionType = VersionType.Major,
            Recipe = originalRecipe.Recipe,
            Steps = originalRecipe.Steps,
            Ingredients = originalRecipe.Ingredients,
            Message = Rand.Primitive.String()
        });
        Assert.Empty(versionId4.ErrorMessage);

        var latest = await Common.Version.GetLatest(recipe.Data!.RecipeId, conn);

        var model = new RecipeModel();
        await model.OnGet(versionId3.Data!.Value);
        Assert.NotNull(model.Details);
        Assert.NotEqual(model.Details.Version.Message, latest.Message);
        Assert.Equal(4, model.Versions.Count);
    }
}