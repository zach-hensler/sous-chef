using core.Data;
using core.Models;
using Xunit;
using Helpers;
using services;

namespace ServiceTests;

public class RecipeServiceTests: Sequential {
    [Fact]
    public async Task ShouldCreateNewRecipe() {
        _ = await Setup.ResetAndGetDatabase();
        var requested = Rand.Domain.Requests.CreateRecipeRequest();
        var createRes = await RecipeService.CreateRecipe(requested);
        Assert.Empty(createRes.ErrorMessage);

        var storedRes = await RecipeService.GetRecipe(createRes.Data!.RecipeId);
        Assert.Empty(storedRes.ErrorMessage);
        Assert.NotNull(storedRes.Data);
        
        Assert.Equal(requested.Recipe.Name, storedRes.Data.RecipeMetadata.Name);
        Assert.Equal(requested.Recipe.Description, storedRes.Data.RecipeMetadata.Description);
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
        _ = await Setup.ResetAndGetDatabase();
        var createRes = await RecipeService.CreateRecipe(Rand.Domain.Requests.CreateRecipeRequest());
        Assert.Empty(createRes.ErrorMessage);
        
        var updated = Rand.Domain.Requests.CreateRecipeRequest();
        await RecipeService.UpdateRecipeVersion(createRes.Data!.VersionId, updated);
        var storedRes = await RecipeService.GetRecipe(createRes.Data!.RecipeId);
        Assert.Empty(storedRes.ErrorMessage);
        Assert.NotNull(storedRes.Data);
        
        Assert.Equal(updated.Recipe.Name, storedRes.Data.RecipeMetadata.Name);
        Assert.Equal(updated.Recipe.Description, storedRes.Data.RecipeMetadata.Description);
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

    [Fact]
    public async Task ShouldCreateMajorRecipeVersion() {
        _ = await Setup.ResetAndGetDatabase();
        var recipe = Rand.Domain.Requests.CreateRecipeRequest();
        var createRecipeRes = await RecipeService.CreateRecipe(recipe);
        Assert.Empty(createRecipeRes.ErrorMessage);

        var newVersionRes = await RecipeService.CreateRecipeVersion(new CreateRecipeVersionRequest {
            PreviousVersionId = createRecipeRes.Data!.VersionId,
            VersionType = VersionType.Major,
            Recipe = recipe.Recipe,
            Steps = recipe.Steps,
            Ingredients = recipe.Ingredients
        });
        Assert.Empty(newVersionRes.ErrorMessage);

        var latest = await RecipeService.GetRecipe(createRecipeRes.Data!.RecipeId);
        Assert.Empty(latest.ErrorMessage);
        Assert.Equal("2.0", latest.Data?.Version.VersionNumber);
    }

    [Fact]
    public async Task ShouldListRecipes() {
        await using var conn = (await Setup.ResetAndGetDatabase()).GetConnection();
        _ = await RecipeService.CreateRecipe(Rand.Domain.Requests.CreateRecipeRequest());
        _ = await RecipeService.CreateRecipe(Rand.Domain.Requests.CreateRecipeRequest());
        var recipe3Data = Rand.Domain.Requests.CreateRecipeRequest();
        var recipe3 = await RecipeService.CreateRecipe(recipe3Data);
        
        await conn.OpenAsync(TestContext.Current.CancellationToken);
        var latest = await Common.RecipeVersion.GetLatest(recipe3.Data!.RecipeId, conn);

        await RecipeService.CreateRecipeVersion(new CreateRecipeVersionRequest {
            PreviousVersionId = latest.VersionId,
            VersionType = VersionType.Major,
            Recipe = recipe3Data.Recipe,
            Steps = recipe3Data.Steps,
            Ingredients = recipe3Data.Ingredients
        });

        var listed = await RecipeService.ListRecipes(new ListRecipesRequest {
            Count = 10,
            Offset = 0
        });
        Assert.Empty(listed.ErrorMessage);
        Assert.NotNull(listed.Data?.Items);
        Assert.Equal(3, listed.Data?.Total);
        foreach (var recipe in listed.Data!.Items) {
            Assert.NotEmpty(recipe.LatestVersionNumber);
            Assert.NotEmpty(recipe.Name);
            Assert.NotEmpty(recipe.Description ?? "");
            Assert.NotEqual(0, recipe.RecipeId.Value);
            if (recipe.RecipeId == recipe3.Data!.RecipeId) {
                Assert.Equal("2.0", recipe.LatestVersionNumber);
            }
        }
    }
}