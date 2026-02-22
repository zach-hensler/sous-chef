namespace core.Models;

public record GetCommentsResponse {
    public required int CommentId { get; init; }
    public string? Comment { get; init; }
    public required int Rating { get; init; }
    public required DateTime CreatedAt { get; init; }
}

public record GetCommentsRequest {
    public required int VersionId { get; init; }
}
