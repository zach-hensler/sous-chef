namespace core.Models.DbModels;

public record WishlistDb {
    public required WishlistId WishlistId { get; init; }
    public required string Name { get; init; }
    public required int Priority { get; init; }
    public string? Reference { get; init; }
    public required bool Completed { get; init; }
    public required DateTime CreatedAt { get; init; }

    public UpdateWishlistDb ToUpdateWishlistDb() {
        return new UpdateWishlistDb {
            WishlistId = WishlistId,
            Priority = Priority,
            Reference = Reference,
            Completed = Completed
        };
    }
}

public record AddWishlistDb {
    public required string Name { get; init; }
    public required int Priority { get; init; }
    public string? Reference { get; init; }
    public required bool Completed { get; set; }
}

public record UpdateWishlistDb {
    public required WishlistId WishlistId { get; init; }
    public required int Priority { get; init; }
    public string? Reference { get; init; }
    public required bool Completed { get; init; }
}

