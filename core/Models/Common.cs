using core.Models.DbModels;

namespace core.Models;

public record CreateRecipeDb {
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required string? OriginalAuthor { get; init; }
    public required int TotalTimeMinutes { get; init; }
    public required int ActiveTimeMinutes { get; init; }
    public required EffortLevels EffortLevel { get; init; }
    public required Categories Category { get; init; }
}

public record CreateRecipeVersionDb {
    public required string Message { get; init; } 
    public required RecipeId RecipeId { get; init; }
    public required DateTime CreatedAt { get; init; }
}

public record CreateRecipeStepDb {
    public required string Name { get; init; }
    public required string Instruction { get; init; }
}

public record CreateRecipeIngredientDb {
    public required string Name { get; init; }
    public required string Note { get; init; }
    public required float Quantity { get; init; }
    public string? Unit { get; init; }
}

public record CreateRecipeCommentDb {
    public required VersionId VersionId { get; init; }
    public required int Rating { get; init; }
    public string? Comment { get; init; }
    public required DateTime CreatedAt { get; init; }
    
}
