using core.Data;
using core.Models;
using Xunit;
using Helpers;
using services;

namespace ServiceTests;

public class RecipeServiceTests: Sequential {
    [Fact]
    public async Task ShouldCreateNewRecipe() {
        await Setup.ResetAndSetupDatabase();
        var requested = Rand.Domain.Requests.CreateRecipeRequest();
        var createRes = await RecipeService.CreateRecipe(requested);
        Assert.NotNull(createRes);

        var storedRes = await RecipeService.GetRecipe(createRes.RecipeId);
        Assert.NotNull(storedRes);
        Assert.NotNull(storedRes);
        
        Assert.Equal(requested.Recipe.Name, storedRes.RecipeMetadata.Name);
        Assert.Equal(requested.Recipe.Description, storedRes.RecipeMetadata.Description);
        Assert.Equal(requested.Ingredients.Count, storedRes.Ingredients.Count);
        foreach (var ingredient in requested.Ingredients) {
            Assert.NotNull(storedRes.Ingredients.Find(stored =>
                stored.Name == ingredient.Name &&
                Math.Abs(stored.Quantity - ingredient.Quantity) < .1f &&
                stored.Unit == ingredient.Unit));
        }

        var stepCounter = 0;
        foreach (var step in requested.Steps) {
            Assert.Equal(step.Name, storedRes.Steps[stepCounter].Name);
            Assert.Equal(step.Instruction, storedRes.Steps[stepCounter].Instruction);
            stepCounter++;
        }
    }
    
    [Fact]
    public async Task ShouldUpdateRecipe() {
        await Setup.ResetAndSetupDatabase();
        var createRes = await RecipeService.CreateRecipe(Rand.Domain.Requests.CreateRecipeRequest());
        Assert.NotNull(createRes);
        
        var updated = Rand.Domain.Requests.CreateRecipeRequest();
        await RecipeService.UpdateRecipeVersion(createRes.VersionId, updated);
        var storedRes = await RecipeService.GetRecipe(createRes.RecipeId);
        Assert.NotNull(storedRes);
        
        Assert.Equal(updated.Recipe.Name, storedRes.RecipeMetadata.Name);
        Assert.Equal(updated.Recipe.Description, storedRes.RecipeMetadata.Description);
        Assert.Equal(updated.Ingredients.Count, storedRes.Ingredients.Count);
        foreach (var ingredient in updated.Ingredients) {
            Assert.NotNull(storedRes.Ingredients.Find(stored =>
                stored.Name == ingredient.Name &&
                Math.Abs(stored.Quantity - ingredient.Quantity) < .1f &&
                stored.Unit == ingredient.Unit));
        }

        var stepCounter = 0;
        foreach (var step in updated.Steps) {
            Assert.Equal(step.Name, storedRes.Steps[stepCounter].Name);
            Assert.Equal(step.Instruction, storedRes.Steps[stepCounter].Instruction);
            stepCounter++;
        }
    }

    [Fact]
    public async Task ShouldCreateMajorRecipeVersion() {
        await using var conn = await Setup.ResetAndGetDatabase();
        var recipe = Rand.Domain.Requests.CreateRecipeRequest();
        var createRecipeRes = await RecipeService.CreateRecipe(recipe);
        Assert.NotNull(createRecipeRes);

        var message = Rand.Primitive.String();
        var newVersionRes = await RecipeService.CreateRecipeVersion(new CreateRecipeVersionRequest {
            PreviousVersionId = createRecipeRes.VersionId,
            VersionType = VersionType.Major,
            Recipe = recipe.Recipe,
            Steps = recipe.Steps,
            Ingredients = recipe.Ingredients,
            Message = message
        });
        Assert.NotNull(newVersionRes);

        var latest = await RecipeService.GetRecipe(createRecipeRes.RecipeId);
        Assert.NotNull(latest);
        Assert.Equal(message, latest.Version.Message);
    }

    [Fact]
    public async Task ShouldListRecipes() {
        await using var conn = await Setup.ResetAndGetDatabase();
        _ = await RecipeService.CreateRecipe(Rand.Domain.Requests.CreateRecipeRequest());
        _ = await RecipeService.CreateRecipe(Rand.Domain.Requests.CreateRecipeRequest());
        var recipe3Data = Rand.Domain.Requests.CreateRecipeRequest();
        var recipe3 = await RecipeService.CreateRecipe(recipe3Data);

        var latest = await Common.Version.GetLatest(recipe3!.RecipeId, conn);

        await RecipeService.CreateRecipeVersion(new CreateRecipeVersionRequest {
            PreviousVersionId = latest.VersionId,
            VersionType = VersionType.Major,
            Recipe = recipe3Data.Recipe,
            Steps = recipe3Data.Steps,
            Ingredients = recipe3Data.Ingredients,
            Message = Rand.Primitive.String()
        });

        var listed = await RecipeService.ListRecipes(new ListRecipesRequest {
            Count = 10,
            Offset = 0
        });
        Assert.NotNull(listed);
        Assert.NotNull(listed.Items);
        Assert.Equal(3, listed.Total);
        foreach (var recipe in listed.Items) {
            Assert.NotEmpty(recipe.Name);
            Assert.NotEmpty(recipe.Description ?? "");
            Assert.NotEqual(0, recipe.RecipeId.Value);
        }
    }
}