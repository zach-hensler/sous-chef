using core.Data;
using core.Models.DbModels;

namespace services;

public static class WishlistService {
    public static async Task<WishlistId?> Add(AddWishlistDb req) {
        return await Utils.SafeRun(nameof(WishlistService), async conn =>
            await Common.Wishlist.Add(req, conn));
    }
    public static async Task<List<WishlistDb>> List() {
        return
            await Utils.SafeRun(nameof(WishlistService), async conn =>
                await Common.Wishlist.List(conn))
            ?? [];
    }
    public static async Task<WishlistId?> Update(UpdateWishlistDb req) {
        return await Utils.SafeRun(nameof(WishlistService), async conn =>
            await Common.Wishlist.Update(req, conn));
    }

    public static async Task<WishlistDb?> Get(WishlistId id) {
        return await Utils.SafeRun(nameof(WishlistService), async conn =>
            await Common.Wishlist.Get(id, conn));
    }
}