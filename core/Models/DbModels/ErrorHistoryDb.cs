namespace core.Models.DbModels;

public record ErrorHistoryDb {
    public required string Source { get; init; }
    public required string Message { get; init; }
    public required DateTime OccurredAt { get; init; }
    public virtual bool Equals(ErrorHistoryDb? other) {
        return
            Source == other?.Source &&
            Message == other.Message &&
            OccurredAt.ToString() == other.OccurredAt.ToString();
    }

    public override int GetHashCode() {
        return HashCode.Combine(Source, Message, OccurredAt);
    }
}
