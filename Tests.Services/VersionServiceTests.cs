using core.Data;
using core.Models.DbModels;
using core.Models.ServiceModels;
using Helpers;
using services;
using Xunit;

namespace ServiceTests;

public class VersionServiceTests: Sequential {
    [Fact]
    public async Task ShouldLeaveComments() {
        await Setup.ResetAndSetupDatabase();
        var createRes = await RecipeService.CreateRecipe(Rand.Domain.Requests.CreateRecipeRequest());

        const int commentCount = 4;
        for (var i = 0; i < 4; i++) {
            await VersionService.AddComment(new CreateRecipeCommentDb {
                VersionId = createRes!.VersionId,
                Rating = Rand.Primitive.Int(0, 5),
                Comment = Rand.Primitive.String(),
                CreatedAt = Rand.Primitive.Date()
            });
        }

        var comments = await RecipeService.GetComments(createRes!.RecipeId);
        Assert.Equal(commentCount, comments.Count);
    }

    [Fact]
    public async Task ShouldDeleteLatestVersion() {
        await using var conn = await Setup.ResetAndGetDatabase();
        
        var v1Res = await RecipeService.CreateRecipe(Rand.Domain.Requests.CreateRecipeRequest());
        Assert.NotNull(v1Res);

        var v2 = await RecipeService.CreateRecipeVersion(
            Rand.Domain.Requests.CreateRecipeVersionRequest(v1Res.VersionId));
        Assert.NotNull(v2);

        var list = await Common.Version.List(v1Res.RecipeId, conn);
        Assert.Equal(2, list.Count);
        
        await VersionService.DeleteRecipeVersion(v2);
        list = await Common.Version.List(v1Res.RecipeId, conn);
        Assert.Single(list);
    }

    [Fact]
    public async Task ShouldDeleteRecipeForSingleVersion() {
        await Setup.ResetAndSetupDatabase();
        var createR1Res = await RecipeService.CreateRecipe(Rand.Domain.Requests.CreateRecipeRequest());
        Assert.NotNull(createR1Res);
        var createR2Res = await RecipeService.CreateRecipe(Rand.Domain.Requests.CreateRecipeRequest());
        Assert.NotNull(createR2Res);

        var list = await RecipeService.ListRecipes(new ListRecipesRequest());
        Assert.NotNull(list);
        Assert.Equal(2, list.Items.Count);
        await VersionService.DeleteRecipeVersion(createR2Res.VersionId);
        
        list = await RecipeService.ListRecipes(new ListRecipesRequest());
        Assert.NotNull(list);
        Assert.Single(list.Items);
    }

    [Fact]
    public async Task ShouldDeleteEntireRecipe() {
        await using var conn = await Setup.ResetAndGetDatabase();
        
        var v1Res = await RecipeService.CreateRecipe(Rand.Domain.Requests.CreateRecipeRequest());
        Assert.NotNull(v1Res);

        var v2 = await RecipeService.CreateRecipeVersion(
            Rand.Domain.Requests.CreateRecipeVersionRequest(v1Res.VersionId));
        Assert.NotNull(v2);

        var list = await ListRecipesData.Get(new ListRecipesRequest(), conn);
        Assert.Single(list);
        
        await VersionService.DeleteEntireRecipe(v2);
        list = await ListRecipesData.Get(new ListRecipesRequest(), conn);
        Assert.Empty(list);
    }
}