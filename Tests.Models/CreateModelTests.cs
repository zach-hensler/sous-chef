using core.Models;
using core.Models.DbModels;
using Helpers;
using services;
using sous_chef.Pages;
using Xunit;

namespace PageTests;

public class CreateModelTests: Sequential {
    [Fact]
    public async Task ShouldCreateNewRecipe() {
        await Setup.ResetAndSetupDatabase();
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
        model.PageContext = Setup.GetPageContext(nameof(CreateActions.SaveExisting));
        await model.OnPostAsync(null);

        var listRecipes = await RecipeService.ListRecipes(new ListRecipesRequest {
            Count = 10,
            Offset = 0
        });
        Assert.NotNull(listRecipes);
        Assert.Single(listRecipes.Items);

        var details = await RecipeService.GetRecipe(listRecipes.Items.First().RecipeId);
        Assert.NotNull(details);
        Assert.Equal(3, details.Steps.Count);
    }

    [Fact]
    public async Task ShouldUpdateExistingRecipe() {
        await Setup.ResetAndSetupDatabase();
        var original = Rand.Domain.Requests.CreateRecipeRequest();
        var recipe = await RecipeService.CreateRecipe(original);

        var model = new CreateModel();
        await model.OnGet(recipe?.VersionId.Value);
        model.RecipeIngredients.Add(Rand.Domain.Db.RecipeIngredientDb().ToViewIngredient());
        model.RecipeSteps.Add(Rand.Domain.Db.RecipeStepDb(original.Steps.Count).ToViewStep());
        var newName = "new_" + Rand.Primitive.String();
        Assert.NotNull(model.RecipeMetadata);
        model.RecipeMetadata = new CreateRecipeView {
            Name = newName,
            Description = model.RecipeMetadata.Description,
            EffortLevel = model.RecipeMetadata.EffortLevel,
            Category = model.RecipeMetadata.Category,
            TotalTime = model.RecipeMetadata.TotalTime,
            ActiveTime = model.RecipeMetadata.ActiveTime
        };
        model.PageContext = Setup.GetPageContext(nameof(CreateActions.SaveExisting));
        await model.OnPostAsync(recipe?.VersionId.Value);

        var details = await RecipeService.GetRecipe(recipe!.RecipeId);
        Assert.NotNull(details);
        Assert.Equal(newName, details.RecipeMetadata.Name);
        Assert.Equal(original.Steps.Count + 1, details.Steps.Count);
        Assert.Equal(original.Ingredients.Count + 1, details.Ingredients.Count);
    }

    [Fact]
    public async Task ShouldCreateNewVersion() {
        await Setup.ResetAndSetupDatabase();
        var original = Rand.Domain.Requests.CreateRecipeRequest();
        var recipe = await RecipeService.CreateRecipe(original);
        Assert.NotNull(recipe);

        var model = new CreateModel();
        await model.OnGet(recipe.VersionId.Value);

        model.RecipeIngredients.Add(Rand.Domain.Db.RecipeIngredientDb().ToViewIngredient());
        model.RecipeSteps.Add(Rand.Domain.Db.RecipeStepDb(original.Steps.Count).ToViewStep());
        var newName = "new_" + Rand.Primitive.String();
        Assert.NotNull(model.RecipeMetadata);
        model.RecipeMetadata = new CreateRecipeView {
            Name = newName,
            Description = model.RecipeMetadata.Description,
            EffortLevel = model.RecipeMetadata.EffortLevel,
            Category = model.RecipeMetadata.Category,
            TotalTime = model.RecipeMetadata.TotalTime,
            ActiveTime = model.RecipeMetadata.ActiveTime
        };
        var message = Rand.Primitive.String();
        model.UpdateMessage = message;

        model.PageContext = Setup.GetPageContext(nameof(CreateActions.SaveAsNewVersion));
        await model.OnPostAsync(recipe.VersionId.Value);

        var details = await RecipeService.GetRecipe(recipe.RecipeId);
        Assert.NotNull(details);
        Assert.Equal(message, details.Version.Message);
        Assert.Equal(newName, details.RecipeMetadata.Name);
    }

    [Fact]
    public async Task ShouldMoveSteps() {
        var model = new CreateModel {
            PageContext = Setup.GetPageContext(nameof(CreateActions.NewStep))
        };
        await model.OnPostAsync(null);
        Assert.Single(model.RecipeSteps);
        model.RecipeSteps[0] = new CreateStepView {
            Name = "name0",
            Instruction = "instruct0"
        };
        
        model.PageContext = Setup.GetPageContext(nameof(CreateActions.NewStep));
        await model.OnPostAsync(null);
        model.RecipeSteps[1] = new CreateStepView {
            Name = "name1",
            Instruction = "instruct1"
        };
        
        model.PageContext = Setup.GetPageContext(nameof(CreateActions.MoveStepUp), "1");
        await model.OnPostAsync(null);
        
        Assert.Equal("name1",model.RecipeSteps[0].Name);
        Assert.Equal("instruct1",model.RecipeSteps[0].Instruction);
        Assert.Equal("name0",model.RecipeSteps[1].Name);
        Assert.Equal("instruct0",model.RecipeSteps[1].Instruction);
    }
}