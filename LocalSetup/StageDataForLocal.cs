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
                
                for (var stepCount = 0; stepCount < Rand.Primitive.Int(1, 10); stepCount++) {
                    await Common.RecipeSteps.Create(
                        Rand.Domain.Db.RecipeStepDb(stepCount + 1).ToCreateStepDb(),
                        stepCount + 1,
                        versionId,
                        conn);
                }

                for (var ingrCount = 0; ingrCount < Rand.Primitive.Int(3, 8); ingrCount++) {
                    await Common.RecipeIngredients.Create(
                        Rand.Domain.Db.RecipeIngredientDb().ToCreateIngredientDb(), versionId, conn);
                }
            }
        }
    }
}