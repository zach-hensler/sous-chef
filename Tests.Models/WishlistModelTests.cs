using core.Models.DbModels;
using Helpers;
using view.Pages.Wishlist;
using Xunit;

namespace PageTests;

public class WishlistModelTests {
    private async Task<WishlistModel> GetModel() {
        return new WishlistModel {
            PageContext = Setup.GetPageContext(),
            MetadataProvider = Setup.MetadataProvider(),
        };
    }
    
    [Fact]
    public async Task ShouldAddToWishlist() {
        await Setup.ResetAndSetupDatabase();
        var model = await GetModel();

        await model.OnGet();
        Assert.Empty(model.Items);

        var created = Rand.Domain.Db.WishlistDb().ToAddWishlistDb();
        await model.OnPostAdd(created);
        Assert.Single(model.Items);
        
        Assert.Contains(model.Items, i => i.Name == created.Name);
    }

    [Fact]
    public async Task ShouldUpdateWishlistItem() {
        await Setup.ResetAndSetupDatabase();
        var model = await GetModel();

        var created = Rand.Domain.Db.WishlistDb().ToAddWishlistDb();
        await model.OnPostAdd(created);
        Assert.Single(model.Items);

        var updated = new UpdateWishlistView(model.Items.First().ToUpdateWishlistDb()) {
            Priority = 2,
            Reference = Rand.Primitive.String()
        };
        await model.OnPostUpdate(updated);
        UpdateWishlistDb[] expected = [updated.ToDb()];
        // Confirm Model updates in the UI
        Assert.Equivalent(expected, model.Items);

        // use new model to be sure it updated in db
        var newModel = await GetModel();
        await newModel.OnGet();
        Assert.Equivalent(expected, newModel.Items);
    }

    [Fact]
    public async Task ShouldHideCompletedItems() {
        await Setup.ResetAndSetupDatabase();
        var model = await GetModel();

        var created = Rand.Domain.Db.WishlistDb().ToAddWishlistDb();
        await model.OnPostAdd(created);
        Assert.Single(model.Items);

        var updated = new UpdateWishlistView(model.Items.First().ToUpdateWishlistDb()) {
            Completed = "on"
        };
        await model.OnPostUpdate(updated);
        // Confirm Model updates in the UI
        Assert.Empty(model.Items);

        // use new model to be sure it updated in db
        var newModel = await GetModel();
        await newModel.OnGet();
        Assert.Empty(newModel.Items);
        
    }
}