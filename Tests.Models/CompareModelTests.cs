using core.Models.DbModels;
using Helpers;
using services;
using view.Pages;
using Xunit;

namespace PageTests;

public class CompareModelTests {
    private CompareModel GetModel() {
        return new CompareModel {
            PageContext = Setup.GetPageContext(),
            MetadataProvider = Setup.MetadataProvider()
        };
    }
    [Theory]
    [InlineData(new string[] {}, new string[] {})]
    [InlineData(new[] {"123"}, new string[] {})]
    [InlineData(new[] {"123", "345"}, new string[] {})]
    [InlineData(new string[] {}, new[] {"abc"})]
    [InlineData(new string[] {}, new[] {"abc", "def"})]
    [InlineData(new[] {"123"}, new[] {"abc", "def"})]
    [InlineData(new[] {"123", "345"}, new[] {"abc"})]
    [InlineData(new[] {"123", "345"}, new[] {"abc", "def"})]
    public async Task ShouldListAllComments(string[] oldComments, string[] newComments) {
        await using var conn = await Setup.ResetAndGetDatabase();

        var originalRecipe = await RecipeService.CreateRecipe(Rand.Domain.Requests.CreateRecipeRequest());
        Assert.NotNull(originalRecipe);

        foreach (var comment in oldComments) {
            var db = Rand.Domain.Db.RecipeCommentDb(originalRecipe.VersionId).ToCreateCommentDb();
            db.Comment = comment;
            await VersionService.AddComment(db);
        }

        var newVersion = await RecipeService.CreateRecipeVersion(
            Rand.Domain.Requests.CreateRecipeVersionRequest(originalRecipe.VersionId));
        Assert.NotNull(newVersion);
        
        foreach (var comment in newComments) {
            var db = Rand.Domain.Db.RecipeCommentDb(newVersion).ToCreateCommentDb();
            db.Comment = comment;
            await VersionService.AddComment(db);
        }

        var model = GetModel();
        await model.OnGet(originalRecipe.VersionId.Value, newVersion.Value);
        
        Assert.Equal(Math.Max(oldComments.Length, newComments.Length), model.CommentComparisons.Count);
        foreach (var comment in oldComments) {
            Assert.Contains(model.CommentComparisons, c => c.Item1Line2 == comment);
        }
        foreach (var comment in newComments) {
            Assert.Contains(model.CommentComparisons, c => c.Item2Line2 == comment);
        }
    }

    // TODO test for changed units and quantities
    [Theory]
    // no change
    [InlineData(new string[] {}, new string[] {})]
    [InlineData(new[] {"onion"}, new[] {"onion"})]
    [InlineData(new[] {"onion", "pasta", "tomato"}, new[] {"pasta", "onion", "tomato"})]
    // add
    [InlineData(new string[] {}, new[] {"onion"})]
    [InlineData(new string[] {}, new[] {"onion", "pasta"})]
    [InlineData(new[] {"onion"}, new[] {"apple", "pasta", "onion"})]
    //remove
    [InlineData(new[] {"onion"}, new string[] {})]
    [InlineData(new[] {"onion", "pasta"}, new string[] {})]
    [InlineData(new[] {"onion", "apple", "pasta"}, new[] {"onion"})]
    // add + remove
    [InlineData(new[] {"onion"}, new[] {"apple", "pasta"})]
    [InlineData(new[] {"apple", "pasta"}, new[] {"onion"})]
    public async Task ShouldCompareIngredients(string[] oldIngredients, string[] newIngredients) {
        await using var conn = await Setup.ResetAndGetDatabase();

        var createRecipe = Rand.Domain.Requests.CreateRecipeRequest();
        createRecipe.Ingredients =
            oldIngredients.Select(i => new CreateIngredientDb {
                    Name = i,
                    Quantity = 1,
                    Unit = "cup"
                })
                .ToList();
        var originalRecipe = await RecipeService.CreateRecipe(createRecipe);
        Assert.NotNull(originalRecipe);

        var createNewVersion = Rand.Domain.Requests.CreateRecipeVersionRequest(originalRecipe.VersionId);
        createNewVersion.Ingredients =
            newIngredients.Select(i => new CreateIngredientDb {
                    Name = i,
                    Quantity = 1,
                    Unit = "cup"
                })
                .ToList();
        var newVersion = await RecipeService.CreateRecipeVersion(createNewVersion);
        Assert.NotNull(newVersion);
        
        var model = GetModel();
        await model.OnGet(originalRecipe.VersionId.Value, newVersion.Value);
        
        // confirms that everything is in the comparison list
        foreach (var ingredient in oldIngredients) {
            Assert.Contains(model.IngredientComparisons, i => i.Item1Line1 == Format(ingredient));
        }
        foreach (var ingredient in newIngredients) {
            Assert.Contains(model.IngredientComparisons, i => i.Item2Line1 == Format(ingredient));
        }

        // confirms that the comparison list types are correct
        foreach (var item in model.IngredientComparisons) {
            CompareHelpers.ComparisonType expectedType;
            if (item.Item1Line1 == null) {
                expectedType = CompareHelpers.ComparisonType.Added;
            }
            else if (item.Item2Line1 == null) {
                expectedType = CompareHelpers.ComparisonType.Deleted;
            }
            else {
                expectedType = CompareHelpers.ComparisonType.Same;
            }
            Assert.Equal(expectedType, item.Type);
        }

        return;

        string Format(string name) => $"{name} 1 cup";
    }

    // TODO test for ordering
    // TODO test for re-ordered
    // TODO test for modified steps
    [Theory]
    // no change
    [InlineData(new string[] {}, new string[] {})]
    [InlineData(new[] {"Prep Veggies", "Make Sauce", "Cook Pasta"}, new [] {"Prep Veggies", "Make Sauce", "Cook Pasta"})]
    // add
    [InlineData(new string[] {}, new [] {"Prep Veggies"})]
    [InlineData(new[] {"Make Sauce", "Cook Pasta"}, new [] {"Prep Veggies", "Make Sauce", "Cook Pasta"})] // start
    [InlineData(new[] {"Prep Veggies", "Make Sauce"}, new [] {"Prep Veggies", "Make Sauce", "Cook Pasta"})] // end
    [InlineData(new[] {"Prep Veggies", "Cook Pasta"}, new [] {"Prep Veggies", "Make Sauce", "Cook Pasta"})] // middle
    [InlineData(new [] {"Make Sauce"}, new [] {"Prep Veggies", "Make Sauce", "Cook Pasta"})] // multi
    // remove
    [InlineData(new [] {"Prep Veggies"}, new string[] {})]
    [InlineData(new[] {"Prep Veggies", "Make Sauce", "Cook Pasta"}, new [] {"Make Sauce", "Cook Pasta"})] // start
    [InlineData(new[] {"Prep Veggies", "Make Sauce", "Cook Pasta"}, new [] {"Prep Veggies", "Make Sauce"})] // end
    [InlineData(new[] {"Prep Veggies", "Make Sauce", "Cook Pasta"}, new [] {"Prep Veggies", "Cook Pasta"})] // middle
    [InlineData(new[] {"Prep Veggies", "Make Sauce", "Cook Pasta"}, new [] {"Prep Veggies"})] // multi
    // add + remove
    [InlineData(new[] {"Prep Veggies", "Make Sauce"}, new [] {"Make Sauce", "Cook Pasta"})]
    [InlineData(new[] {"Make Sauce", "Cook Pasta"}, new [] {"Prep Veggies", "Make Sauce"})]
    [InlineData(new[] {"Prep Veggies", "Cook Pasta"}, new [] {"Prep Veggies", "Make Sauce"})]
    public async Task ShouldCompareSteps(string[] oldSteps, string[] newSteps) {
        await using var conn = await Setup.ResetAndGetDatabase();

        var createRecipe = Rand.Domain.Requests.CreateRecipeRequest();
        createRecipe.Steps =
            oldSteps.Select(s => new CreateStepDb {
                    Name = s,
                    Instruction = ""
                })
                .ToList();
        var originalRecipe = await RecipeService.CreateRecipe(createRecipe);
        Assert.NotNull(originalRecipe);

        var createNewVersion = Rand.Domain.Requests.CreateRecipeVersionRequest(originalRecipe.VersionId);
        createNewVersion.Steps =
            newSteps.Select(s => new CreateStepDb { 
                    Name = s,
                    Instruction = ""
                })
                .ToList();
        var newVersion = await RecipeService.CreateRecipeVersion(createNewVersion);
        Assert.NotNull(newVersion);
        
        var model = GetModel();
        await model.OnGet(originalRecipe.VersionId.Value, newVersion.Value);
        
        // confirms that everything is in the comparison list
        foreach (var step in oldSteps) {
            Assert.Contains(model.StepComparisons, s => s.Item1Line1?.Contains(step) ?? false);
        }
        foreach (var step in newSteps) {
            Assert.Contains(model.StepComparisons, s => s.Item2Line1?.Contains(step) ?? false);
        }

        // confirms that the comparison list types are correct
        foreach (var item in model.StepComparisons) {
            CompareHelpers.ComparisonType expectedType;
            if (item.Item1Line1 == null) {
                expectedType = CompareHelpers.ComparisonType.Added;
            }
            else if (item.Item2Line1 == null) {
                expectedType = CompareHelpers.ComparisonType.Deleted;
            }
            else {
                expectedType = CompareHelpers.ComparisonType.Same;
            }
            Assert.Equal(expectedType, item.Type);
        }
    }
}