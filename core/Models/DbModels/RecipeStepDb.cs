using core.Models.ViewModels;

namespace core.Models.DbModels;

public record RecipeStepDb {
    public required RecipeId RecipeId { get; init; }
    public required VersionId VersionId { get; init; }
    public required string Name { get; init; }
    public required string StepNumber { get; init; }
    public required string Instruction { get; init; }

    public ViewStep ToViewStep() {
        return new ViewStep {
            Name = Name,
            Instruction = Instruction
        };
    }
}
