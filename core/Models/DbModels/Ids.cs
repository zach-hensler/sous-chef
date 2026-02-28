namespace core.Models.DbModels;

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

public record RecipeId(int Value) : IdBase(Value);
public record VersionId(int Value) : IdBase(Value);
