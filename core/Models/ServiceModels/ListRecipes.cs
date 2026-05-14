using core.Models.DbModels;

namespace core.Models.ServiceModels;

public record ListRecipesResponse {
    public record RecipeItem {
        public required RecipeId RecipeId { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public required EffortLevels EffortLevel { get; init; }
        public required Categories Category { get; init; }
        public required VersionId LatestVersionId { get; init; }
        public required float AvgScore { get; init; }
    }
    public required List<RecipeItem> Items { get; init; }
}

public record ListRecipesRequest {
    public Categories? CategoryFilter { get; set; }
}