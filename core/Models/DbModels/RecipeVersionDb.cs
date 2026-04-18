namespace core.Models.DbModels;

public record RecipeVersionDb {
    public required VersionId VersionId { get; init; }
    public required string Message { get; init; }
    public required RecipeId RecipeId { get; init; }
    public required DateTime CreatedAt { get; init; }

    public CreateRecipeVersionDb ToCreateVersionDb() {
        return new CreateRecipeVersionDb {
            Message = Message,
            RecipeId = RecipeId,
            CreatedAt = CreatedAt
        };
    }
}

public record CreateRecipeVersionDb {
    public required string Message { get; init; } 
    public required RecipeId RecipeId { get; init; }
    public required DateTime CreatedAt { get; init; }
}
