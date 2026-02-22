using core.Models.ViewModels;

namespace core.Models.DbModels;

public record RecipeStepDb {
    public required int RecipeId { get; init; }
    public required int VersionId { get; init; }
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
