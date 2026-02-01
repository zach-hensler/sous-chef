using core;
using core.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using services;

namespace sous_chef.Pages;

public class RecipeDetailsModel : PageModel {
    private readonly RecipeService _recipeService = new RecipeService(new ConnectionFactory());
    public RecipeDetails? Details { get; set; }

    public async Task OnGet(int id) {
        var res = await _recipeService.GetRecipe(id);
        if ((int)res.StatusCode < 300) {
            Details = res.Data;
        }
        else {
            Console.WriteLine(res.ErrorMessage);
        }
    }
}