using core.Data;
using core.Models.DbModels;
using Helpers;
using Xunit;

namespace DataTests;

public class WishlistTests: Sequential {
    [Fact]
    public async Task ShouldAddItemsToWishlist() {
        await using var conn = await Setup.ResetAndGetDatabase();

        var newWishlist = new AddWishlistDb {
            Name = Rand.Primitive.String(),
            Priority = Rand.Primitive.Int(2, 5),
            Reference = Rand.Primitive.String(),
            Completed = false
        };
        var id = await Common.Wishlist.Add(newWishlist, conn);
        var created = await Common.Wishlist.Get(id, conn);

        Assert.Equivalent(newWishlist, created);
    }

    [Fact]
    public async Task ShouldUpdateWishlistItems() {
        await using var conn = await Setup.ResetAndGetDatabase();

        var id = await Common.Wishlist.Add(Rand.Domain.Wishlist.AddWishlistDb(), conn);
        var updateReq = new UpdateWishlistDb {
            WishlistId = id,
            Priority = 7,
            Reference = "https://example.com",
            Completed = true
        };
        await Common.Wishlist.Update(updateReq, conn);
        var updated = await Common.Wishlist.Get(id, conn);
        
        Assert.Equal(updateReq.Priority, updated.Priority);
        Assert.Equal(updateReq.Reference, updated.Reference);
        Assert.Equal(updateReq.Completed, updated.Completed);
    }

    [Fact]
    public static async Task ShouldntListCompletedItems() {
        await using var conn = await Setup.ResetAndGetDatabase();
        
        var notCompleted = await Common.Wishlist.Add(new AddWishlistDb {
            Name = "Banana",
            Priority = 1,
            Reference = "",
            Completed = false
        }, conn);

        var req = Rand.Domain.Wishlist.AddWishlistDb();
        req.Completed = true;
        await Common.Wishlist.Add(req, conn);

        req = Rand.Domain.Wishlist.AddWishlistDb();
        req.Completed = true;
        await Common.Wishlist.Add(req, conn);

        var list = await Common.Wishlist.List(conn);
        Assert.Single(list);
        Assert.Equal(notCompleted, list.First().WishlistId);
    }

    [Fact]
    public static async Task ShouldListItemsByPriorityThenName() {
        await using var conn = await Setup.ResetAndGetDatabase();

        var id3 = await Common.Wishlist.Add(new AddWishlistDb {
            Name = "Apple",
            Priority = 1,
            Reference = "",
            Completed = false
        }, conn);

        var id1 = await Common.Wishlist.Add(new AddWishlistDb {
            Name = "Banana",
            Priority = 3,
            Reference = "",
            Completed = false
        }, conn);

        var id2 = await Common.Wishlist.Add(new AddWishlistDb {
            Name = "Butter",
            Priority = 2,
            Reference = "",
            Completed = false
        }, conn);

        var id4 = await Common.Wishlist.Add(new AddWishlistDb {
            Name = "Grape",
            Priority = 1,
            Reference = "",
            Completed = false
        }, conn);

        var list = await Common.Wishlist.List(conn);
        Assert.Equal([id1, id2, id3, id4], list.Select(w => w.WishlistId)) ;
    }
}