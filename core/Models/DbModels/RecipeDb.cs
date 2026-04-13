using core.Models.ViewModels;

namespace core.Models.DbModels;

public record RecipeDb {
    public required RecipeId RecipeId { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required string? OriginalAuthor { get; init; }
    public required int TotalTimeMinutes { get; init; }
    public required int ActiveTimeMinutes { get; init; }
    public required EffortLevels EffortLevel { get; init; }
    public required Categories Category { get; init; }

    public CreateRecipeView ToViewRecipe() {
        return new CreateRecipeView {
            Name = Name,
            Description = Description,
            EffortLevel = EffortLevel,
            Category = Category,
            TotalTime = TotalTimeMinutes,
            OriginalAuthor = OriginalAuthor,
            ActiveTime = ActiveTimeMinutes
        };
    }
}