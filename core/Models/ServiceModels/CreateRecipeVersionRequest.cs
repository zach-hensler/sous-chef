using System.Text.Json.Serialization;
using core.Models.DbModels;

namespace core.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VersionType {
    Major,
    Minor
}

public class CreateRecipeVersionRequest {
    public required string Message { get; init; }
    public required VersionId PreviousVersionId { get; init; }
    public required VersionType VersionType { get; init; }
    public required CreateRecipeDb Recipe { get; init; }
    public required List<CreateRecipeStepDb> Steps { get; init; }
    public required List<CreateRecipeIngredientDb> Ingredients { get; init; }
}