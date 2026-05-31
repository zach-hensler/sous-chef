using core.Models.DbModels;

namespace core.Models.ServiceModels;

public record CreateRecipeRequest {
    public required CreateRecipeDb Recipe { get; init; }
    public required List<CreateStepDb> Steps { get; set; }
    public required List<CreateIngredientDb> Ingredients { get; set; }
}
