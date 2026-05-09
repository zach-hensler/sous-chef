using core.Models.DbModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using services;

namespace sous_chef.Pages.Wishlist;

public record UpdateWishlistView {
    public required int WishlistId { get; init; }
    public required int Priority { get; init; }
    public string? Reference { get; init; }
    public string? Completed { get; init; }
    public UpdateWishlistDb ToDb() {
        return new UpdateWishlistDb {
            WishlistId = new WishlistId(WishlistId),
            Priority = Priority,
            Reference = Reference,
            Completed = Completed == "on" // HTML form post returns this when checked
        };
    }
}

public class WishlistModel : PageModel {
    public List<WishlistDb> Items { get; set; } = [];

    [BindProperty] public AddWishlistDb NewWishlistItem { get; set; } = new() {
        Name = "",
        Priority = 1,
        Reference = null,
        Completed = false
    };

    [BindProperty] public WishlistDb? EditWishlistItem { get; set; }

    public async Task OnGet() {
        Items = await WishlistService.List();
    }
    
    public async Task<IActionResult> OnGetEdit(int id) {
        EditWishlistItem =
            await WishlistService.Get(new WishlistId(id))
            ?? throw new Exception($"Unable to find item with id '{id}'");
        return Partial("_WishlistEdit", this);
    }

    public async Task<IActionResult> OnPostUpdate(UpdateWishlistView? model) {
        if (model == null) {
            return Partial("_WishlistEdit", this);
        }

        var updated = await WishlistService.Update(model.ToDb());
        if (updated == null) {
            return Partial("_WishlistEdit", this);
        }

        Items = await WishlistService.List();
        return Partial("_WishlistEditClose", this);
    }

    public IActionResult OnGetAdd() {
        return Partial("_WishlistAdd", this);
    }

    public async Task<IActionResult> OnPostAdd(AddWishlistDb? model) {
        if (model == null) {
            return Partial("_WishlistAdd", this);
        }

        var added = await WishlistService.Add(model);
        if (added == null) {
            return Partial("_WishlistAdd", this);
        }
    
        Items = await WishlistService.List();
        return Partial("_WishlistAddClose", this);
    }
}