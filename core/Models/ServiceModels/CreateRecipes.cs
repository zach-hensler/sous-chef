using core.Models.DbModels;

namespace core.Models;

public record CreateRecipeRequest {
    public required CreateRecipeDb Recipe { get; init; }
    public required List<CreateStepDb> Steps { get; init; }
    public required List<CreateIngredientDb> Ingredients { get; init; }
}
