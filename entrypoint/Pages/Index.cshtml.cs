using core;
using core.Models.ServiceModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using services;

namespace sous_chef.Pages;

public class IndexModel : PageModel {
    public ListRecipesResponse? RecipeData { get; set; }
    public ListRecipesRequest RecipesRequest = new() {
        CategoryFilter = null
    };

    public async Task OnGet() {
        RecipeData = await RecipeService.ListRecipes(RecipesRequest);
    }

    public async Task<IActionResult> OnGetWithFilter([FromQuery] string? category) {
        RecipesRequest.CategoryFilter =
            category != null ? Enum.Parse<Categories>(category, ignoreCase: true) : null;
        RecipeData = await RecipeService.ListRecipes(RecipesRequest);
        return Partial("_RecipeList", this);
    }
}