using System.Net;
using core;
using core.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using services;

namespace sous_chef.Pages;

public class IndexModel : PageModel {
    public ListRecipesResponse? RecipeData { get; set; }
    public ListRecipesRequest? RecipesRequest = new() {
        Count = 100,
        Offset = 0
    };

    public async Task OnGet() {
        if (RecipesRequest == null) {
            return;
        }
        RecipeData = await RecipeService.ListRecipes(RecipesRequest);
    }
}