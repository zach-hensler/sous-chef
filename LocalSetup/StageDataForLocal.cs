using core.Data;
using Helpers;
using Xunit;

namespace LocalSetup;

public class StageDataForLocal: Sequential {
    [Fact]
    public async Task ShouldStageGeneralData() {
        await using var conn = await Setup.ResetAndGetDatabase();

        for (var recipeCount = 0; recipeCount < Rand.Primitive.Int(10, 20); recipeCount++) {
            var recipeId = await Common.Recipe.Create(Rand.Domain.Db.RecipeDb().ToCreateRecipeDb(), conn);
            for (var versionCount = 0; versionCount < Rand.Primitive.Int(1, 3); versionCount++) {
                var versionId = await Common.Version.Create(
                    Rand.Domain.Db.RecipeVersionDb(recipeId).ToCreateVersionDb(), conn);
                
                for (var stepCount = 0; stepCount < Rand.Primitive.Int(2, 10); stepCount++) {
                    await Common.RecipeSteps.Create(
                        Rand.Domain.Db.RecipeStepDb(stepCount + 1).ToCreateStepDb(),
                        stepCount + 1,
                        versionId,
                        conn);
                }

                for (var ingrCount = 0; ingrCount < Rand.Primitive.Int(3, 8); ingrCount++) {
                    var ingredient = Rand.Domain.Db.RecipeIngredientDb().ToCreateIngredientDb();
                    await Common.RecipeIngredients.Create(ingredient, versionId, conn);
                }

                for (var commentCount = 0; commentCount < Rand.Primitive.Int(0, 4); commentCount++) {
                    await Common.RecipeComments.Create(
                        Rand.Domain.Db.RecipeCommentDb(versionId).ToCreateCommentDb(), conn);
                }
            }
        }

        for (var wishlistCount = 0; wishlistCount < Rand.Primitive.Int(3, 7); wishlistCount++) {
            await Common.Wishlist.Add(Rand.Domain.Db.WishlistDb().ToAddWishlistDb(), conn);
        }
    }
}