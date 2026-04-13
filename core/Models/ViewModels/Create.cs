namespace core.Models.ViewModels;

public record ViewIngedient {
    public required string Name { get; init; }
    public required float Quantity { get; init; }
    public string? Unit { get; init; }

    public CreateRecipeIngredientDb ToIngredientDb() {
        return new CreateRecipeIngredientDb {
            Name = Name,
            Note = "",
            Quantity = Quantity,
            Unit = Unit
        };
    }
}

public class CreateStepView {
    public required string Name { get; init; }
    public required string Instruction { get; init; }

    public CreateRecipeStepDb ToStepDb() {
        return new CreateRecipeStepDb {
            Name = Name,
            Instruction = Instruction
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