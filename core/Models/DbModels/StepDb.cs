namespace core.Models.DbModels;

public record StepDb {
    public required RecipeId RecipeId { get; init; }
    public required VersionId VersionId { get; init; }
    public required string Name { get; init; }
    public required string StepNumber { get; init; }
    public required string Instruction { get; init; }

    public CreateStepView ToViewStep() {
        return new CreateStepView {
            Name = Name,
            Instruction = Instruction
        };
    }

    public CreateStepDb ToCreateStepDb() {
        return new CreateStepDb {
            Name = Name,
            Instruction = Instruction
        };
    }
}

public class CreateStepView {
    public required string Name { get; init; }
    public required string Instruction { get; init; }

    public CreateStepDb ToStepDb() {
        return new CreateStepDb {
            Name = Name,
            Instruction = Instruction
        };
    }
}

public record CreateStepDb {
    public required string Name { get; init; }
    public required string Instruction { get; init; }
}
