using System.Text.Json.Serialization;

namespace core;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EffortLevels {
    Easy,
    Medium,
    Hard
}
