using System.Text.Json.Serialization;

namespace core;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EffortLevels {
    Easy,
    Medium,
    Hard
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Categories {
    Uncategorized,
    Entree,
    Side,
    Drink,
    Desert
}
