using System.Text.Json.Serialization;

namespace core.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VersionType {
    Major,
    Minor
}

public class CreateRecipeVersionRequest {
    public required int PreviousVersionId { get; init; }
    public required VersionType VersionType { get; init; }
    public required CreateRecipeDb Recipe { get; init; }
    public required List<CreateRecipeStepDb> Steps { get; init; }
    public required List<CreateRecipeIngredientDb> Ingredients { get; init; }
}