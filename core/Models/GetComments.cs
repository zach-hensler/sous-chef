namespace core.Models;

public record GetCommentsResponse {
    public required int CommentId { get; init; }
    public string? Comment { get; init; }
    public required int Rating { get; init; }
    public required DateTime CreatedAt { get; init; }

    public GetCommentsResponse FromDb(RecipeCommentDb db) {
        return new GetCommentsResponse {
            CommentId = db.CommentId,
            Comment = db.Comment,
            Rating = db.Rating,
            CreatedAt = db.CreatedAt
        };
    }
}

public record GetCommentsRequest {
    public required int VersionId { get; init; }
}
