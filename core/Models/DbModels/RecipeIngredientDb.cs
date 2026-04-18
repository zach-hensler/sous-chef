namespace core.Models.DbModels;

public record RecipeIngredientDb {
    public required RecipeId RecipeId { get; init; }
    public required VersionId VersionId { get; init; }
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

    public CreateIngredientDb ToCreateIngredientDb() {
        return new CreateIngredientDb {
            Name = Name,
            Note = Note,
            Quantity = Quantity,
            Unit = Unit
        };
    }
}

public record CreateIngredientDb {
    public required string Name { get; init; }
    public required string Note { get; init; }
    public required float Quantity { get; init; }
    public string? Unit { get; init; }
}

public record ViewIngedient {
    public required string Name { get; init; }
    public required float Quantity { get; init; }
    public string? Unit { get; init; }

    public CreateIngredientDb ToIngredientDb() {
        return new CreateIngredientDb {
            Name = Name,
            Note = "",
            Quantity = Quantity,
            Unit = Unit
        };
    }
}
