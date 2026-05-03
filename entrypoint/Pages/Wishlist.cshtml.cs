using System.Text.Json.Serialization;
using core.Models.DbModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using services;

namespace sous_chef.Pages;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WishlistActions {
    Add
}

public class WishlistModel : PageModel {
    public List<WishlistDb> Items { get; set; } = [];

    [BindProperty]
    public AddWishlistDb NewWishlistItem { get; set; } = new() {
        Name = "",
        Priority = 1,
        Reference = null,
        Completed = false
    };

    public async Task OnGet() {
        Items = await WishlistService.List();
    }

    public async Task<IActionResult> OnPost() {
        foreach (var kvp in Request.Query) {
            if (!Enum.TryParse<WishlistActions>(kvp.Key, out var action)) {
                return Page();
            }

            return action switch {
                WishlistActions.Add => await HandleAdd(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        return Page();

        async Task<IActionResult> HandleAdd() {
            if (string.IsNullOrWhiteSpace(NewWishlistItem.Name)) {
                return Page();
            }

            if (await WishlistService.Add(NewWishlistItem) == null) {
                return Page();
            }
            return RedirectToPage("Wishlist");
        }
    }
}