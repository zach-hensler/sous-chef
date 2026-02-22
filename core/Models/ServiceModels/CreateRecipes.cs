namespace core.Models;

public record CreateRecipeRequest {
    public required CreateRecipeDb Recipe { get; init; }
    public required List<CreateRecipeStepDb> Steps { get; init; }
    public required List<CreateRecipeIngredientDb> Ingredients { get; init; }
}
