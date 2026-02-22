using core.Models.ViewModels;

namespace core.Models.DbModels;

public record RecipeIngredientDb {
    public required int RecipeId { get; init; }
    public required int VersionId { get; init; }
    public required string Name { get; init; }
    public required string Note { get; init; }
    public required float Quantity { get; init; }
    public required string Unit { get; init; }

    public ViewIngedient ToViewIngredient() {
        return new ViewIngedient {
            Name = Name,
            Quantity = Quantity,
            Unit = Unit
        };
    }
}
