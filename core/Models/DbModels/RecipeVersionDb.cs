namespace core.Models.DbModels;

public record RecipeVersionDb {
    public required int VersionId { get; init; }
    public required string VersionNumber { get; init; }
    public required int RecipeId { get; init; }
    public required DateTime CreatedAt { get; init; }
}
