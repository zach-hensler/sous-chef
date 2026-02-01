using System.Net;
using core;
using core.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using services;

namespace sous_chef.Pages;

public class IndexModel : PageModel {
    private RecipeService _recipeService = new(new ConnectionFactory());

    public (HttpStatusCode, string)? Error { get; set; }
    
    public ListRecipesResponse? RecipeData { get; set; }
    public ListRecipesRequest? RecipesRequest = new() {
        Count = 100,
        Offset = 0
    };

    public async Task OnGet() {
        var res = await _recipeService.ListRecipes(RecipesRequest);
        if ((int)res.StatusCode <= 300) {
            RecipeData = res.Data;
        }
        else {
            Error = (res.StatusCode, res.ErrorMessage);
        }
    }
}