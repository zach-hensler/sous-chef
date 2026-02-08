namespace core.Models;

public record GetCommentsResponse {
    public required int CommentId { get; init; }
    public string? Comment { get; init; }
    public required int Rating { get; init; }
    public required DateTime CreatedAt { get; init; }

    public GetCommentsResponse FromDb(RecipeCommentDb db) {
        return new GetCommentsResponse {
            CommentId = db.comment_id,
            Comment = db.comment,
            Rating = db.rating,
            CreatedAt = db.created_at
        };
    }
}

public record GetCommentsRequest {
    public required int VersionId { get; init; }
}
