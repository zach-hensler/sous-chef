using core.Models.DbModels;
using core.Models.ServiceModels;
using Helpers;
using services;
using view.Pages;
using view.Pages.Create;
using Xunit;

namespace PageTests;

public class CreateModelTests: Sequential {
    #region save
    
    [Fact]
    public async Task ShouldCreateNewRecipe() {
        await Setup.ResetAndSetupDatabase();
        var model = new CreateModel {
            RecipeMetadata = Rand.Domain.Db.RecipeDb().ToCreateRecipeDb(),
            Ingredients = [
                Rand.Domain.Db.RecipeIngredientDb().ToCreateIngredientDb(),
                Rand.Domain.Db.RecipeIngredientDb().ToCreateIngredientDb()
            ],
            Steps = [
                Rand.Domain.Db.RecipeStepDb(1).ToCreateStepDb(),
                Rand.Domain.Db.RecipeStepDb(2).ToCreateStepDb(),
                Rand.Domain.Db.RecipeStepDb(3).ToCreateStepDb()
            ]
        };
        await model.OnGet(null);
        model.PageContext = Setup.GetPageContext();
        await model.OnPostSaveExisting(null, model.RecipeMetadata, model.Ingredients, model.Steps);

        var listRecipes = await RecipeService.ListRecipes(new ListRecipesRequest());
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
        model.Ingredients.Add(Rand.Domain.Db.RecipeIngredientDb().ToCreateIngredientDb());
        model.Steps.Add(Rand.Domain.Db.RecipeStepDb(original.Steps.Count).ToCreateStepDb());
        var newName = "new_" + Rand.Primitive.String();
        Assert.NotNull(model.RecipeMetadata);
        model.RecipeMetadata.Name = newName;
        model.PageContext = Setup.GetPageContext();
        await model.OnPostSaveExisting(recipe?.VersionId.Value, model.RecipeMetadata, model.Ingredients, model.Steps);

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

        model.Ingredients.Add(Rand.Domain.Db.RecipeIngredientDb().ToCreateIngredientDb());
        model.Steps.Add(Rand.Domain.Db.RecipeStepDb(original.Steps.Count).ToCreateStepDb());
        var newName = "new_" + Rand.Primitive.String();
        Assert.NotNull(model.RecipeMetadata);
        model.RecipeMetadata.Name = newName;
        var message = Rand.Primitive.String();
        model.UpdateMessage = message;

        model.PageContext = Setup.GetPageContext();
        await model.OnPostSaveNew(recipe.VersionId.Value, model.RecipeMetadata, model.Ingredients, model.Steps);

        var details = await RecipeService.GetRecipe(recipe.RecipeId);
        Assert.NotNull(details);
        Assert.Equal(message, details.Version.Message);
        Assert.Equal(newName, details.RecipeMetadata.Name);
    }
    
    #endregion
    
    #region steps

    [Theory]
    [InlineData(Direction.Up, 1)]
    [InlineData(Direction.Down, 0)]
    public void ShouldMoveSteps(Direction direction, int index) {
        var model = new CreateModel {
            PageContext = Setup.GetPageContext(),
            MetadataProvider = Setup.MetadataProvider()
        };
        model.OnPostAddStep(model.Steps);
        Assert.Single(model.Steps);
        model.Steps[0] = new CreateStepDb {
            Name = "name0",
            Instruction = "instruct0"
        };

        model.OnPostAddStep(model.Steps);
        model.Steps[1] = new CreateStepDb {
            Name = "name1",
            Instruction = "instruct1"
        };

        model.OnPostMoveStep(model.Steps, index, direction);
        
        Assert.Equal("name1",model.Steps[0].Name);
        Assert.Equal("instruct1",model.Steps[0].Instruction);
        Assert.Equal("name0",model.Steps[1].Name);
        Assert.Equal("instruct0",model.Steps[1].Instruction);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void ShouldRemoveSteps(int removedIndex) {
        var model = new CreateModel {
            PageContext = Setup.GetPageContext(),
            MetadataProvider = Setup.MetadataProvider(),
            Steps = [
                Rand.Domain.Db.RecipeStepDb(1).ToCreateStepDb(),
                Rand.Domain.Db.RecipeStepDb(2).ToCreateStepDb(),
                Rand.Domain.Db.RecipeStepDb(3).ToCreateStepDb(),
                Rand.Domain.Db.RecipeStepDb(4).ToCreateStepDb()
            ]
        };

        var initialCount = model.Steps.Count;
        var removed = model.Steps[removedIndex];
        model.OnPostRemoveStep(model.Steps, removedIndex);
        Assert.Equal(initialCount - 1, model.Steps.Count);
        Assert.DoesNotContain(model.Steps, s => s.Name == removed.Name);
    }
    
    #endregion

    #region ingredients

    [Fact]
    public void ShouldAddIngredients() {
        var model = new CreateModel {
            PageContext = Setup.GetPageContext(),
            MetadataProvider = Setup.MetadataProvider()
        };

        model.OnPostAddIngredient(model.Ingredients);
        Assert.Single(model.Ingredients);

        model.OnPostAddIngredient(model.Ingredients);
        Assert.Equal(2, model.Ingredients.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void ShouldRemoveIngredients(int removedIndex) {
        var model = new CreateModel {
            PageContext = Setup.GetPageContext(),
            MetadataProvider = Setup.MetadataProvider(),
            Ingredients = [
                Rand.Domain.Db.RecipeIngredientDb().ToCreateIngredientDb(),
                Rand.Domain.Db.RecipeIngredientDb().ToCreateIngredientDb(),
                Rand.Domain.Db.RecipeIngredientDb().ToCreateIngredientDb(),
                Rand.Domain.Db.RecipeIngredientDb().ToCreateIngredientDb()
            ]
        };
        var initialCount = model.Ingredients.Count;

        var removedName = model.Ingredients[removedIndex].Name;
        model.OnPostRemoveIngredient(model.Ingredients, removedIndex);
        Assert.Equal(initialCount - 1, model.Ingredients.Count);
        Assert.DoesNotContain(model.Ingredients, i => i.Name == removedName);
    }

    #endregion
}