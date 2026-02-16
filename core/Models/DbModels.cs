namespace core.Models;

public record RecipeDb {
    public required int recipe_id { get; init; }
    public required string name { get; init; }
    public string? description { get; init; }
    public required int time_minutes { get; init; }
    public required EffortLevels effort_level { get; init; }
    public required Categories category { get; init; }
}

public record RecipeVersionDb {
    public required int version_id { get; init; }
    public required string version_number { get; init; }
    public required int recipe_id { get; init; }
    public required DateTime created_at { get; init; }
}

public record RecipeCommentDb {
    public required int comment_id { get; init; }
    public required int version_id { get; init; }
    public required int rating { get; init; }
    public string? comment { get; init; }
    public required DateTime created_at { get; init; }
}

public record RecipeStepDb {
    public required int recipe_id { get; init; }
    public required int version_id { get; init; }
    public required string name { get; init; }
    public required string step_number { get; init; }
    public required string instruction { get; init; }
}

public record RecipeIngredientDb {
    public required int recipe_id { get; init; }
    public required int version_id { get; init; }
    public required string name { get; init; }
    public required string note { get; init; }
    public required float quantity { get; init; }
    public required string unit { get; init; }
}

public record ErrorHistoryDb {
    public required string source { get; init; }
    public required string message { get; init; }
    public required DateTime occurred_at { get; init; }
    public virtual bool Equals(ErrorHistoryDb? other) {
        return
            source == other?.source &&
            message == other.message &&
            occurred_at.ToString() == other.occurred_at.ToString();
    }

    public override int GetHashCode() {
        return HashCode.Combine(source, message, occurred_at);
    }
}
