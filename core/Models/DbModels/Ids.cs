using System.Text.Json;
using System.Text.Json.Serialization;

namespace core.Models.DbModels;

public class IdConverter<T> : JsonConverter<T> where T : IdBase {
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        var value = reader.GetInt32();
        var constructor = typeToConvert.GetConstructor([typeof(int)]);

        if (constructor == null) {
            throw new JsonException($"{typeToConvert.Name} has no constructor for (int)");
        }

        return (T)constructor.Invoke([value]) ?? throw new JsonException("");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) {
        writer.WriteNumberValue(value.Value);
    }
}

public record IdBase(int Value) {
    public virtual bool Equals(IdBase? other) {
        return Value == other?.Value;
    }

    public override int GetHashCode() {
        return Value;
    }

    public sealed override string ToString() {
        return Value.ToString();
    }
}

// Make sure to register any new ids w/ the dapper mapper
[JsonConverter(typeof(IdConverter<RecipeId>))]
public record RecipeId(int Value) : IdBase(Value);

[JsonConverter(typeof(IdConverter<VersionId>))]
public record VersionId(int Value) : IdBase(Value);

[JsonConverter(typeof(IdConverter<WishlistId>))]
public record WishlistId(int Value) : IdBase(Value);
