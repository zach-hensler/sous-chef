namespace core.Models.DbModels;

public record RecipeDb {
    public required RecipeId RecipeId { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required string? OriginalAuthor { get; init; }
    public required int TotalTimeMinutes { get; init; }
    public required int ActiveTimeMinutes { get; init; }
    public required EffortLevels EffortLevel { get; init; }
    public required Categories Category { get; init; }

    public CreateRecipeView ToViewRecipe() {
        return new CreateRecipeView {
            Name = Name,
            Description = Description,
            EffortLevel = EffortLevel,
            Category = Category,
            TotalTime = TotalTimeMinutes,
            OriginalAuthor = OriginalAuthor,
            ActiveTime = ActiveTimeMinutes
        };
    }

    public CreateRecipeDb ToCreateRecipeDb() {
        return new CreateRecipeDb {
            Name = Name,
            Description = Description,
            OriginalAuthor = OriginalAuthor,
            TotalTimeMinutes = TotalTimeMinutes,
            ActiveTimeMinutes = ActiveTimeMinutes,
            EffortLevel = EffortLevel,
            Category = Category
        };
    }
}

public record CreateRecipeView {
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required EffortLevels EffortLevel { get; init; }
    public required Categories Category { get; init; } = Categories.Uncategorized;
    public string? OriginalAuthor { get; init; }
    public required int TotalTime { get; init; }
    public required int ActiveTime { get; init; }

    public CreateRecipeDb ToRecipeDb() {
        return new CreateRecipeDb {
            Name = Name,
            Description = Description,
            EffortLevel = EffortLevel,
            Category = Category,
            OriginalAuthor = OriginalAuthor,
            TotalTimeMinutes = TotalTime,
            ActiveTimeMinutes = ActiveTime
        };
    }
}

public record CreateRecipeDb {
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required string? OriginalAuthor { get; init; }
    public required int TotalTimeMinutes { get; init; }
    public required int ActiveTimeMinutes { get; init; }
    public required EffortLevels EffortLevel { get; init; }
    public required Categories Category { get; set; }
}