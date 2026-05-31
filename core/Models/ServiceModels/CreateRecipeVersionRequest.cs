using System.Text.Json.Serialization;
using core.Models.DbModels;

namespace core.Models;

public class CreateRecipeVersionRequest {
    public required string Message { get; init; }
    public required VersionId PreviousVersionId { get; init; }
    public required CreateRecipeDb Recipe { get; init; }
    public required List<CreateStepDb> Steps { get; set; }
    public required List<CreateIngredientDb> Ingredients { get; set; }
}