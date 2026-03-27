namespace core.Models.DbModels;

public record RecipeVersionDb {
    public required VersionId VersionId { get; init; }
    public required string Message { get; init; }
    public required RecipeId RecipeId { get; init; }
    public required DateTime CreatedAt { get; init; }
}
