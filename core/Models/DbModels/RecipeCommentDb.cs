namespace core.Models.DbModels;

public record RecipeCommentDb {
    public required int CommentId { get; init; }
    public required VersionId VersionId { get; init; }
    public required int Rating { get; set; }
    public string? Comment { get; init; }
    public required DateTime CreatedAt { get; init; }

    public CreateCommentDb ToCreateCommentDb() {
        return new CreateCommentDb {
            VersionId = VersionId,
            Rating = Rating,
            Comment = Comment,
            CreatedAt = CreatedAt
        };
    }
}

public record CreateCommentDb {
    public required VersionId VersionId { get; init; }
    public required int Rating { get; init; }
    public string? Comment { get; init; }
    public required DateTime CreatedAt { get; init; }
}
