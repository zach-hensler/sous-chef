using core.Models.DbModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using services;

namespace sous_chef.Pages;

public class WishlistEditModal : PageModel {
    public WishlistId? Id { get; set; }
    [BindProperty] public WishlistDb? Item { get; set; }
    public async Task OnGet(int wishlistId) {
        Id = new WishlistId(wishlistId);
        Item = await WishlistService.Get(Id);
    }

    public async Task<IActionResult> OnPost(int wishlistId) {
        Id = new WishlistId(wishlistId);
        if (Item == null) {
            return Page();
        }

        var result = await WishlistService.Update(new UpdateWishlistDb {
            WishlistId = Id,
            Priority = Item.Priority,
            Reference = Item.Reference,
            Completed = Item.Completed
        });

        if (result == null) {
            return Page();
        }
        return Redirect("/Wishlist");
    }
}