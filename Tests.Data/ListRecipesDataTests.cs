using core;
using core.Data;
using core.Models.DbModels;
using core.Models.ServiceModels;
using Helpers;
using services;
using Xunit;

namespace DataTests;

public class ListRecipesDataTests: Sequential {
    [Theory]
    [InlineData(null, 3)]
    [InlineData(Categories.Entree, 1)]
    [InlineData(Categories.Side, 2)]
    [InlineData(Categories.Desert, 0)]
    public async Task ShouldFilterByCategory(Categories? filterCat, int expectedCount) {
        await using var conn = await Setup.ResetAndGetDatabase();

        var request1 = Rand.Domain.Requests.CreateRecipeRequest();
        request1.Recipe.Category = Categories.Entree;
        await RecipeService.CreateRecipe(request1);
        var request2 = Rand.Domain.Requests.CreateRecipeRequest();
        request2.Recipe.Category = Categories.Side;
        await RecipeService.CreateRecipe(request2);
        var request3 = Rand.Domain.Requests.CreateRecipeRequest();
        request3.Recipe.Category = Categories.Side;
        await RecipeService.CreateRecipe(request3);

        var list = await ListRecipesData.Get(new ListRecipesRequest(filterCat), conn);
        Assert.Equal(expectedCount, list.Count);
    }

    [Theory]
    [InlineData(new int[] {}, 0)]
    [InlineData(new[] {1}, 1)]
    [InlineData(new[] {5}, 5)]
    [InlineData(new[] {2,4}, 3)]
    [InlineData(new[] {2,5,4,1,3,3}, (2+5+4+1+3+3)/6)]
    public async Task ShouldShowAverageScore(int[] scores, float avg) {
        await using var conn = await Setup.ResetAndGetDatabase();
        
        var created = await RecipeService.CreateRecipe(Rand.Domain.Requests.CreateRecipeRequest());
        Assert.NotNull(created);

        foreach (var score in scores) {
            var comment = Rand.Domain.Db.RecipeCommentDb(created.VersionId);
            comment.Rating = score;
            Assert.NotNull(await VersionService.AddComment(comment.ToCreateCommentDb()));
        }

        var list = await ListRecipesData.Get(new ListRecipesRequest(), conn);
        Assert.Equal(avg, list.First().AvgScore);
    }

    [Theory]
    [InlineData]
    [InlineData("apple pie", "carrot cake", "salsa", "taco", "zebra cakes")]
    [InlineData("taco", "salsa", "apple pie", "zebra cakes", "carrot cake")]
    [InlineData("zebra cake", "pesto pasta", "chocolate pie", "banana pudding")]
    public async Task ShouldSortAlphabetically(params string[] names) {
        await using var conn = await Setup.ResetAndGetDatabase();

        foreach (var name in names) {
            var req = Rand.Domain.Requests.CreateRecipeRequest();
            req.Recipe.Name = name;
            await RecipeService.CreateRecipe(req);
        }

        var list = await ListRecipesData.Get(new ListRecipesRequest(SortStrategies.Alphabetical), conn);
        names.Sort();
        Assert.Equal(names, list.Select(r => r.Name));
    }

    [Theory]
    [InlineData(SortStrategies.Newest)]
    [InlineData(SortStrategies.Oldest)]
    [InlineData(SortStrategies.Newest, "first", "second")]
    [InlineData(SortStrategies.Oldest, "first", "second")]
    [InlineData(SortStrategies.Newest, "first", "second", "third", "fourth")]
    [InlineData(SortStrategies.Oldest, "first", "second", "third", "fourth")]
    [InlineData(SortStrategies.Newest, "first", "second", "third", "fourth", "abc", "def", "123", "456")]
    [InlineData(SortStrategies.Oldest, "first", "second", "third", "fourth", "abc", "def", "123", "456")]
    public async Task ShouldSortByAge(SortStrategies sort, params string[] names) {
        await using var conn = await Setup.ResetAndGetDatabase();

        foreach (var name in names) {
            var req = Rand.Domain.Requests.CreateRecipeRequest();
            req.Recipe.Name = name;
            await RecipeService.CreateRecipe(req);
        }

        // force some recipes out of order
        if (names.Length > 3) {
            var req = Rand.Domain.Requests.CreateRecipeRequest();
            const int idx = 2;
            req.Recipe.Name = names[idx];
            await RecipeService.UpdateRecipeVersion(new VersionId(idx + 1), req);
        }

        if (names.Length > 5) {
            var req = Rand.Domain.Requests.CreateRecipeRequest();
            const int idx = 5;
            req.Recipe.Name = names[idx];
            await RecipeService.UpdateRecipeVersion(new VersionId(idx + 1), req);
        }

        var list = await ListRecipesData.Get(new ListRecipesRequest(sort), conn);
        if (sort == SortStrategies.Newest) {
            list.Reverse();
        }
        Assert.Equal(names, list.Select(r => r.Name));
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(6, 0)]
    [InlineData(6, 100)]
    [InlineData(20, 50)]
    public async Task ShouldSortByFavorites(int recipeCount, int newVersionPercentage) {
        await using var conn = await Setup.ResetAndGetDatabase();

        for (var i = 0; i < recipeCount; i++) {
            var created =
                await RecipeService.CreateRecipe(Rand.Domain.Requests.CreateRecipeRequest());
            Assert.NotNull(created);
            await AddComments(created.VersionId);
            if (Rand.Primitive.Bool(newVersionPercentage)) {
                var newVersion = await RecipeService.CreateRecipeVersion(
                    Rand.Domain.Requests.CreateRecipeVersionRequest(created.VersionId));
                Assert.NotNull(newVersion);
                await AddComments(newVersion);
            }
        }

        var list = await ListRecipesData.Get(new ListRecipesRequest(SortStrategies.Favorites), conn);
        for (var idx = 0; idx < list.Count - 1; idx++) {
            Assert.True(list[idx].AvgScore >= list[idx + 1].AvgScore);
        }

        return;

        async Task AddComments(VersionId version) {
            for (var c = 0; c < Rand.Primitive.Int(0, 5); c++) {
                await VersionService.AddComment(
                    Rand.Domain.Db.RecipeCommentDb(version).ToCreateCommentDb());
            }
        }
    }
}