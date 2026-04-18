using core.Data;
using core.Models;
using core.Models.DbModels;
using Helpers;
using services;
using Xunit;

namespace ServiceTests;

public class VersionServiceTests: Sequential {
    [Fact]
    public async Task ShouldLeaveComments() {
        _ = await Setup.ResetAndGetDatabase();
        var createRes = await RecipeService.CreateRecipe(Rand.Domain.Requests.CreateRecipeRequest());

        const int commentCount = 4;
        for (var i = 0; i < 4; i++) {
            await VersionService.AddComment(new CreateRecipeCommentDb {
                VersionId = createRes.Data!.VersionId,
                Rating = Rand.Primitive.Int(0, 5),
                Comment = Rand.Primitive.String(),
                CreatedAt = Rand.Primitive.Date()
            });
        }

        var comments = await RecipeService.GetComments(createRes.Data!.RecipeId);
        Assert.Empty(comments.ErrorMessage);
        Assert.Equal(commentCount, comments.Data?.Count);
    }

    [Fact]
    public async Task ShouldDeleteLatestVersion() {
        await using var conn = (await Setup.ResetAndGetDatabase()).GetConnection();
        
        var v1Res = await RecipeService.CreateRecipe(Rand.Domain.Requests.CreateRecipeRequest());
        Assert.Empty(v1Res.ErrorMessage);

        var v2 = await RecipeService.CreateRecipeVersion(
            Rand.Domain.Requests.CreateRecipeVersionRequest(v1Res.Data!.VersionId));
        Assert.Empty(v2.ErrorMessage);

        await conn.OpenAsync(TestContext.Current.CancellationToken);
        var list = await Common.Version.List(v1Res.Data!.RecipeId, conn);
        Assert.Equal(2, list.Count);
        
        var deleteRes = await VersionService.DeleteRecipeVersion(v2.Data!);
        Assert.Empty(deleteRes.ErrorMessage);
        list = await Common.Version.List(v1Res.Data!.RecipeId, conn);
        Assert.Single(list);
    }

    [Fact]
    public async Task ShouldDeleteRecipeForSingleVersion() {
        _ = await Setup.ResetAndGetDatabase();
        var createR1Res = await RecipeService.CreateRecipe(Rand.Domain.Requests.CreateRecipeRequest());
        Assert.Empty(createR1Res.ErrorMessage);
        var createR2Res = await RecipeService.CreateRecipe(Rand.Domain.Requests.CreateRecipeRequest());
        Assert.Empty(createR2Res.ErrorMessage);

        var list = await RecipeService.ListRecipes(new ListRecipesRequest { Count = 2, Offset = 0 });
        Assert.Empty(list.ErrorMessage);
        Assert.Equal(2, list.Data!.Total);
        await VersionService.DeleteRecipeVersion(createR2Res.Data!.VersionId);
        
        list = await RecipeService.ListRecipes(new ListRecipesRequest { Count = 2, Offset = 0 });
        Assert.Empty(list.ErrorMessage);
        Assert.Equal(1, list.Data!.Total);
    }

    [Fact]
    public async Task ShouldDeleteEntireRecipe() {
        await using var conn = (await Setup.ResetAndGetDatabase()).GetConnection();
        
        var v1Res = await RecipeService.CreateRecipe(Rand.Domain.Requests.CreateRecipeRequest());
        Assert.Empty(v1Res.ErrorMessage);

        var v2 = await RecipeService.CreateRecipeVersion(
            Rand.Domain.Requests.CreateRecipeVersionRequest(v1Res.Data!.VersionId));
        Assert.Empty(v2.ErrorMessage);

        await conn.OpenAsync(TestContext.Current.CancellationToken);
        var list = await ListRecipeData.Get(10, 0, conn);
        Assert.Single(list);
        
        var deleteRes = await VersionService.DeleteEntireRecipe(v2.Data!);
        Assert.Empty(deleteRes.ErrorMessage);
        list = await ListRecipeData.Get(10, 0, conn);
        Assert.Empty(list);
    }
}