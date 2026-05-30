namespace core.Models.DbModels;

public record RecipeIngredientDb {
    public required RecipeId RecipeId { get; init; }
    public required VersionId VersionId { get; init; }
    public required string Name { get; init; }
    public required float Quantity { get; init; }
    public required string Unit { get; init; }

    public CreateIngredientDb ToCreateIngredientDb() {
        return new CreateIngredientDb {
            Name = Name,
            Quantity = Quantity,
            Unit = Unit
        };
    }
}

public record CreateIngredientDb {
    public required string Name { get; init; }
    public required float Quantity { get; init; }
    public string? Unit { get; init; }
}
