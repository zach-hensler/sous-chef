using core.Models.ViewModels;

namespace core.Models.DbModels;

public record RecipeDb {
    public required RecipeId RecipeId { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required int TimeMinutes { get; init; }
    public required EffortLevels EffortLevel { get; init; }
    public required Categories Category { get; init; }

    public ViewRecipe ToViewRecipe() {
        return new ViewRecipe {
            Name = Name,
            Description = Description,
            EffortLevel = EffortLevel,
            Category = Category,
            Time = TimeMinutes
        };
    }
}