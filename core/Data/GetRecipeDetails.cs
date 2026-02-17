using System.Data.Common;

namespace core.Data;

public record RecipeDetails {
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required EffortLevels EffortLevel { get; init; }
    public required Categories Category { get; init; }
    public required int Time { get; init; }
    public required string VersionNumber { get; init; }
    public required List<Step> Steps { get; init; }
    public required List<Ingredient> Ingredients { get; init; }

    public record Step {
        public required string Name { get; init; }
        public required string Instruction { get; init; }
    }

    public record Ingredient {
        public required string Name { get; init; }
        public required float Quantity { get; init; }
        public string? Unit { get; init; }
    }
}

public static class GetRecipeDetails {
    public static async Task<RecipeDetails> Get(int recipeId, DbConnection conn) {
        var recipe = await Common.Recipe.Get(recipeId, conn);
        var latestVersion = await Common.RecipeVersion.GetLatest(recipeId, conn);
        
        var steps = await Common.RecipeSteps.Get(latestVersion.VersionId, conn);
        var ingredients = await Common.RecipeIngredients.Get(latestVersion.VersionId, conn);

        return new RecipeDetails {
            Name = recipe.Name,
            Description = recipe.Description,
            EffortLevel = recipe.EffortLevel,
            Category = recipe.Category,
            Time = recipe.TimeMinutes,
            VersionNumber = latestVersion.VersionNumber,
            Steps =
                steps
                    .OrderBy(s => s.StepNumber)
                    .Select(s => new RecipeDetails.Step {
                        Name = s.Name,
                        Instruction = s.Instruction
                    })
                    .ToList(),
            Ingredients =
                ingredients
                    .Select(i => new RecipeDetails.Ingredient {
                        Name = i.Name,
                        Quantity = i.Quantity,
                        Unit = i.Unit
                    })
                    .ToList()
        };
    }
}