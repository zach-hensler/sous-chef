using core;
using core.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using services;

namespace sous_chef.Pages;

public enum RecipeDetailsAction {
    DeleteConfirmation
}

public class RecipeDetailsModel : PageModel {
    private readonly RecipeService _recipeService = new(new ConnectionFactory());
    public RecipeDetails? Details { get; set; }
    public int Id { get; set; }
    public bool DeleteRequested { get; set; } = false;

    private async Task LoadPageData(int id) {
        var res = await _recipeService.GetRecipe(id);
        if ((int)res.StatusCode < 300) {
            Details = res.Data;
        }
        else {
            Console.WriteLine(res.ErrorMessage);
        }
    }
    
    public async Task OnGet(int id) {
        Id = id;
        await LoadPageData(id);
    }

    public async Task<IActionResult> OnPost(int id) {
        Id = id;
        await LoadPageData(id);
        Request.Query.TryGetValue("action", out var action);
        if (!Enum.TryParse<RecipeDetailsAction>(action.ToString(), out var postAction)) {
            return Page();
        }

        return postAction switch {
            RecipeDetailsAction.DeleteConfirmation => await HandleDelete(id),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public IActionResult HandleDeleteRequest() {
        DeleteRequested = true;
        return Page();
    } 

    public async Task<IActionResult> HandleDelete(int id) {
        var res = await _recipeService.DeleteRecipe(id);
        if ((int)res.StatusCode >= 300) {
            Console.WriteLine(res.ErrorMessage);
        }
        return RedirectToPage("Index");
    }
}