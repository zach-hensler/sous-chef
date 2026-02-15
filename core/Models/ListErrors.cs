namespace core.Models;

public record ListErrorsResponse {
    public required List<ErrorHistoryDb> Errors { get; init; }
    public required int Total { get; init; } 
}