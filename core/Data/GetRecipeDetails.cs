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
        
        var steps = await Common.RecipeSteps.Get(latestVersion.version_id, conn);
        var ingredients = await Common.RecipeIngredients.Get(latestVersion.version_id, conn);

        return new RecipeDetails {
            Name = recipe.name,
            Description = recipe.description,
            EffortLevel = recipe.effort_level,
            Category = recipe.category,
            Time = recipe.time_minutes,
            VersionNumber = latestVersion.version_number,
            Steps =
                steps
                    .OrderBy(s => s.step_number)
                    .Select(s => new RecipeDetails.Step {
                        Name = s.name,
                        Instruction = s.instruction
                    })
                    .ToList(),
            Ingredients =
                ingredients
                    .Select(i => new RecipeDetails.Ingredient {
                        Name = i.name,
                        Quantity = i.quantity,
                        Unit = i.unit
                    })
                    .ToList()
        };
    }
}