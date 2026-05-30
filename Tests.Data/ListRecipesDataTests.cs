using core;
using core.Data;
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

        var list = await ListRecipesData.Get(new ListRecipesRequest { CategoryFilter = filterCat }, conn);
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
}