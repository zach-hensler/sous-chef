using core.Models;
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
    
}