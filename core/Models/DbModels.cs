namespace core.Models;

public record RecipeDb {
    public required int RecipeId { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required int TimeMinutes { get; init; }
    public required EffortLevels EffortLevel { get; init; }
    public required Categories Category { get; init; }
}

public record RecipeVersionDb {
    public required int VersionId { get; init; }
    public required string VersionNumber { get; init; }
    public required int RecipeId { get; init; }
    public required DateTime CreatedAt { get; init; }
}

public record RecipeCommentDb {
    public required int CommentId { get; init; }
    public required int VersionId { get; init; }
    public required int Rating { get; init; }
    public string? Comment { get; init; }
    public required DateTime CreatedAt { get; init; }
}

public record RecipeStepDb {
    public required int RecipeId { get; init; }
    public required int VersionId { get; init; }
    public required string Name { get; init; }
    public required string StepNumber { get; init; }
    public required string Instruction { get; init; }
}

public record RecipeIngredientDb {
    public required int RecipeId { get; init; }
    public required int VersionId { get; init; }
    public required string Name { get; init; }
    public required string Note { get; init; }
    public required float Quantity { get; init; }
    public required string Unit { get; init; }
}

public record ErrorHistoryDb {
    public required string Source { get; init; }
    public required string Message { get; init; }
    public required DateTime OccurredAt { get; init; }
    public virtual bool Equals(ErrorHistoryDb? other) {
        return
            Source == other?.Source &&
            Message == other.Message &&
            OccurredAt.ToString() == other.OccurredAt.ToString();
    }

    public override int GetHashCode() {
        return HashCode.Combine(Source, Message, OccurredAt);
    }
}
