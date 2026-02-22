using core.Models;
using core.Models.ViewModels;
using Helpers;
using Microsoft.Extensions.Primitives;
using services;
using sous_chef.Pages;
using Xunit;

namespace PageTests;

public class CreateModelTests: Sequential {
    [Fact]
    public async Task ShouldCreateNewRecipe() {
        _ = await Setup.ResetAndGetDatabase();
        var model = new CreateModel {
            RecipeMetadata = Rand.Domain.Db.RecipeDb().ToViewRecipe(),
            RecipeIngredients = [
                Rand.Domain.Db.RecipeIngredientDb().ToViewIngredient(),
                Rand.Domain.Db.RecipeIngredientDb().ToViewIngredient()
            ],
            RecipeSteps = [
                Rand.Domain.Db.RecipeStepDb(1).ToViewStep(),
                Rand.Domain.Db.RecipeStepDb(2).ToViewStep(),
                Rand.Domain.Db.RecipeStepDb(3).ToViewStep()
            ]
        };
        await model.OnGet(null);
        model.PageContext = Setup.GetPageContext(new Dictionary<string, StringValues> {
            [nameof(CreateActions.SaveExisting)] = StringValues.Empty
        });
        await model.OnPostAsync(null);

        var listRecipes = await RecipeService.ListRecipes(new ListRecipesRequest {
            Count = 10,
            Offset = 0
        });
        Assert.Empty(listRecipes.ErrorMessage);
        Assert.NotNull(listRecipes.Data);
        Assert.Single(listRecipes.Data.Items);

        var details = await RecipeService.GetRecipe(listRecipes.Data.Items.First().RecipeId);
        Assert.Empty(details.ErrorMessage);
        Assert.NotNull(details.Data);
        Assert.Equal(3, details.Data.Steps.Count);
    }

    [Fact]
    public async Task ShouldUpdateExistingRecipe() {
        _ = await Setup.ResetAndGetDatabase();
        var original = Rand.Domain.Requests.CreateRecipeRequest();
        var recipe = await RecipeService.CreateRecipe(original);
        Assert.Empty(recipe.ErrorMessage);

        var model = new CreateModel();
        await model.OnGet(recipe.Data?.VersionId);
        model.RecipeIngredients.Add(Rand.Domain.Db.RecipeIngredientDb().ToViewIngredient());
        model.RecipeSteps.Add(Rand.Domain.Db.RecipeStepDb(original.Steps.Count).ToViewStep());
        var newName = "new_" + Rand.Primitive.String();
        model.RecipeMetadata = new ViewRecipe {
            Name = newName,
            Description = model.RecipeMetadata!.Description,
            EffortLevel = model.RecipeMetadata!.EffortLevel,
            Category = model.RecipeMetadata!.Category,
            Time = model.RecipeMetadata!.Time
        };
        model.PageContext = Setup.GetPageContext(new Dictionary<string, StringValues> {
            [nameof(CreateActions.SaveExisting)] = StringValues.Empty
        });
        await model.OnPostAsync(recipe.Data?.VersionId);

        var details = await RecipeService.GetRecipe(recipe.Data!.RecipeId);
        Assert.Empty(details.ErrorMessage);
        Assert.NotNull(details.Data);
        Assert.Equal(newName, details.Data.RecipeMetadata.Name);
        Assert.Equal(original.Steps.Count + 1, details.Data.Steps.Count);
        Assert.Equal(original.Ingredients.Count + 1, details.Data.Ingredients.Count);
    }

    [Fact]
    public async Task ShouldCreateNewMinorVersion() {
        _ = await Setup.ResetAndGetDatabase();
        var original = Rand.Domain.Requests.CreateRecipeRequest();
        var recipe = await RecipeService.CreateRecipe(original);
        Assert.Empty(recipe.ErrorMessage);

        var model = new CreateModel();
        await model.OnGet(recipe.Data!.VersionId);
        model.RecipeIngredients.Add(Rand.Domain.Db.RecipeIngredientDb().ToViewIngredient());
        model.RecipeSteps.Add(Rand.Domain.Db.RecipeStepDb(original.Steps.Count).ToViewStep());
        var newName = "new_" + Rand.Primitive.String();
        model.RecipeMetadata = new ViewRecipe {
            Name = newName,
            Description = model.RecipeMetadata!.Description,
            EffortLevel = model.RecipeMetadata!.EffortLevel,
            Category = model.RecipeMetadata!.Category,
            Time = model.RecipeMetadata!.Time
        };
        model.PageContext = Setup.GetPageContext(new Dictionary<string, StringValues> {
            [nameof(CreateActions.SaveAsNewMinorVersion)] = StringValues.Empty
        });
        await model.OnPostAsync(recipe.Data.VersionId);
        var details = await RecipeService.GetRecipe(recipe.Data.VersionId);
        Assert.Empty(details.ErrorMessage);
        Assert.Equal("1.1", details.Data?.Version.VersionNumber);
        Assert.Equal(newName, details.Data?.RecipeMetadata.Name);
    }
    
    [Fact]
    public async Task ShouldCreateNewMajorVersion() {
        _ = await Setup.ResetAndGetDatabase();
        var original = Rand.Domain.Requests.CreateRecipeRequest();
        var recipe = await RecipeService.CreateRecipe(original);
        Assert.Empty(recipe.ErrorMessage);

        var model = new CreateModel();
        await model.OnGet(recipe.Data!.VersionId);
        model.RecipeIngredients.Add(Rand.Domain.Db.RecipeIngredientDb().ToViewIngredient());
        model.RecipeSteps.Add(Rand.Domain.Db.RecipeStepDb(original.Steps.Count).ToViewStep());
        var newName = "new_" + Rand.Primitive.String();
        model.RecipeMetadata = new ViewRecipe {
            Name = newName,
            Description = model.RecipeMetadata!.Description,
            EffortLevel = model.RecipeMetadata!.EffortLevel,
            Category = model.RecipeMetadata!.Category,
            Time = model.RecipeMetadata!.Time
        };
        model.PageContext = Setup.GetPageContext(new Dictionary<string, StringValues> {
            [nameof(CreateActions.SaveAsNewMajorVersion)] = StringValues.Empty
        });
        await model.OnPostAsync(recipe.Data.VersionId);
        var details = await RecipeService.GetRecipe(recipe.Data.VersionId);
        Assert.Empty(details.ErrorMessage);
        Assert.Equal("2.0", details.Data?.Version.VersionNumber);
        Assert.Equal(newName, details.Data?.RecipeMetadata.Name);
    }

    [Fact]
    public async Task ShouldAddNewSteps() {
        _ = await Setup.ResetAndGetDatabase();
        var original = Rand.Domain.Requests.CreateRecipeRequest();
        var recipe = await RecipeService.CreateRecipe(original);
        Assert.Empty(recipe.ErrorMessage);

        var model = new CreateModel();
        await model.OnGet(recipe.Data!.VersionId);
        model.PageContext = Setup.GetPageContext(new Dictionary<string, StringValues> {
            [nameof(CreateActions.SaveAsNewMajorVersion)] = StringValues.Empty
        });
        await model.OnPostAsync(recipe.Data.VersionId);
    }
}