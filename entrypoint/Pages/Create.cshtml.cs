using System.Net;
using core;
using core.Data;
using core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using services;

public class CreateModel : PageModel {
    private RecipeService _recipeService = new(new ConnectionFactory());

    public record Recipe {
        public required string Name { get; init; }
        public required string Description { get; init; }
    }
    
    [BindProperty]
    public Recipe RecipeData { get; set; }

    public void OnGet() {
    }

    public async Task<IActionResult> OnPostAsync() {
        var response = await _recipeService.CreateRecipe(new CreateRecipeRequest {
            Recipe = new CreateRecipeDb {
                Name = RecipeData.Name,
                Description = RecipeData.Description,
                TimeMinutes = 0,
                EffortLevel = EffortLevels.Low
            },
            Steps = [],
            Ingredients = []
        });

        if (response.StatusCode == HttpStatusCode.OK) {
            return RedirectToPage("./Create");
        }

        return Page();
    }
}