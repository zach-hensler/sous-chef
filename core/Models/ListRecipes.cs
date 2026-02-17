namespace core.Models;

public record ListRecipesResponse {
    public record RecipeItem {
        public required int RecipeId { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public required EffortLevels EffortLevel { get; init; }
        public required Categories Category { get; init; }
        public required string VersionNumber { get; init; }
        
    }
    public required int Total { get; init; }
    public required List<RecipeItem> Items { get; init; }
}

public record ListRecipesRequest {
    public required int Count { get; init; }
    public required int Offset { get; init; }
}