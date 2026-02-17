using core.Data;
using core.Models;
using Xunit;
using Helpers;
using services;

namespace ServiceTests;

public class RecipeServiceTests {
    [Fact]
    public async Task ShouldCreateNewRecipe() {
        _ = await Setup.ResetAndGetDatabase();
        var requested = Rand.Domain.CreateRecipeRequest();
        var createRes = await RecipeService.CreateRecipe(requested);
        Assert.Empty(createRes.ErrorMessage);

        var storedRes = await RecipeService.GetRecipe(createRes.Data);
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
        _ = await Setup.ResetAndGetDatabase();
        var createRes = await RecipeService.CreateRecipe(Rand.Domain.CreateRecipeRequest());
        Assert.Empty(createRes.ErrorMessage);
        
        var updated = Rand.Domain.CreateRecipeRequest();
        await RecipeService.UpdateRecipeVersion(createRes.Data, updated);
        var storedRes = await RecipeService.GetRecipe(createRes.Data);
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

    [Fact]
    public async Task ShouldCreateMajorRecipeVersion() {
        _ = await Setup.ResetAndGetDatabase();
        var recipe = Rand.Domain.CreateRecipeRequest();
        var createRecipeRes = await RecipeService.CreateRecipe(recipe);
        Assert.Empty(createRecipeRes.ErrorMessage);

        var newVersionRes = await RecipeService.CreateRecipeVersion(new CreateRecipeVersionRequest {
            PreviousVersionId = createRecipeRes.Data,
            VersionType = VersionType.Major,
            Recipe = recipe.Recipe,
            Steps = recipe.Steps,
            Ingredients = recipe.Ingredients
        });
        Assert.Empty(newVersionRes.ErrorMessage);

        var latest = await RecipeService.GetRecipe(createRecipeRes.Data);
        Assert.Empty(latest.ErrorMessage);
        Assert.Equal("2.0", latest.Data?.VersionNumber);
    }

    [Fact]
    public async Task ShouldLeaveComments() {
        _ = await Setup.ResetAndGetDatabase();
        var createRes = await RecipeService.CreateRecipe(Rand.Domain.CreateRecipeRequest());

        const int commentCount = 4;
        for (var i = 0; i < 4; i++) {
            await RecipeService.AddComment(new CreateRecipeCommentDb {
                VersionId = createRes.Data,
                Rating = Rand.Primitive.Int(0, 5),
                Comment = Rand.Primitive.String(),
                CreatedAt = Rand.Primitive.Date()
            });
        }

        var comments = await RecipeService.GetComments(createRes.Data);
        Assert.Empty(comments.ErrorMessage);
        Assert.Equal(commentCount, comments.Data?.Count);
    }

    [Fact]
    public async Task ShouldListRecipes() {
        var conn = (await Setup.ResetAndGetDatabase()).GetConnection();
        _ = await RecipeService.CreateRecipe(Rand.Domain.CreateRecipeRequest());
        _ = await RecipeService.CreateRecipe(Rand.Domain.CreateRecipeRequest());
        var recipe3Data = Rand.Domain.CreateRecipeRequest();
        var recipe3 = await RecipeService.CreateRecipe(recipe3Data);
        
        await conn.OpenAsync(TestContext.Current.CancellationToken);
        var latest = await Common.RecipeVersion.GetLatest(recipe3.Data, conn);

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
            Assert.NotEmpty(recipe.VersionNumber);
            Assert.NotEmpty(recipe.Name);
            Assert.NotEmpty(recipe.Description ?? "");
            Assert.NotEqual(0, recipe.RecipeId);
            if (recipe.RecipeId == recipe3.Data) {
                Assert.Equal("2.0", recipe.VersionNumber);
            }
        }
    }
}