using System.Text.Json.Serialization;
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

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SortStrategies {
    Favorites,
    Newest,
    Oldest,
    Alphabetical
}
public record ListRecipesRequest {
    public ListRecipesRequest() {}
    public ListRecipesRequest(Categories? category) {
        CategoryFilter = category;
    }
    public ListRecipesRequest(SortStrategies? sort) {
        SortStrategy = sort;
    }
    public Categories? CategoryFilter { get; set; }
    public SortStrategies? SortStrategy { get; set; }
}