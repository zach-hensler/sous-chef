namespace core.Models.DbModels;

public record RecipeCommentDb {
    public required int CommentId { get; init; }
    public required VersionId VersionId { get; init; }
    public required int Rating { get; init; }
    public string? Comment { get; init; }
    public required DateTime CreatedAt { get; init; }
}
