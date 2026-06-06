using core;
using core.Models.ServiceModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using services;

namespace view.Pages;

public class IndexModel : PageModel {
    public ListRecipesResponse? RecipeData { get; set; }
    public ListRecipesRequest RecipesRequest = new() {
        CategoryFilter = null,
        SortStrategy = null
    };

    public async Task OnGet() {
        RecipeData = await RecipeService.ListRecipes(RecipesRequest);
    }

    public async Task<IActionResult> OnGetWithFilter([FromQuery] string? category, [FromQuery] string? order) {
        RecipesRequest.CategoryFilter = category != null ? Enum.Parse<Categories>(category) : null;
        RecipesRequest.SortStrategy = order != null ? Enum.Parse<SortStrategies>(order) : null;
        RecipeData = await RecipeService.ListRecipes(RecipesRequest);
        return Partial("_RecipeList", this);
    }
}