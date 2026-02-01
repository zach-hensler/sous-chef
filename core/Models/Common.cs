namespace core.Models;

public record CreateRecipeDb {
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required int TimeMinutes { get; init; }
    public required EffortLevels EffortLevel { get; init; }
}

public record CreateRecipeVersionDb {
    public required int RecipeId { get; init; }
    public required string VersionNumber { get; init; }
    public required DateTime CreatedAt { get; init; }
}

public record CreateRecipeStepDb {
    public required string Name { get; init; }
    public required string Step { get; init; }
}

public record CreateRecipeIngredientDb {
    public required string Name { get; init; }
    public required string Note { get; init; }
    public required float Quantity { get; init; }
    public string? Unit { get; init; }
}
