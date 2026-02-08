using core;
using core.Data;
using core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using services;

namespace sous_chef.Pages;

public enum RecipeDetailsAction {
    AddComment,
    DeleteConfirmation
}

public record NewComment {
    public required int Rating { get; init; }
    public required string? Comment { get; init; }
}

public class Recipe : PageModel {
    private readonly RecipeService _recipeService = new(new ConnectionFactory());
    public RecipeDetails? Details { get; set; }
    public List<GetCommentsResponse> Comments { get; set; } = [];

    [BindProperty]
    public NewComment NewComment { get; set; } = new() {
        Rating = 0,
        Comment = null
    };

    public int Id { get; set; }

    private async Task LoadPageData(int id) {
        var res = await _recipeService.GetRecipe(id);
        if ((int)res.StatusCode < 300 && res.Data != null) {
            Details = res.Data;
        }
        else {
            Console.WriteLine(res.ErrorMessage);
            return;
        }

        var commentRes = await _recipeService.GetComments(id);
        if ((int)res.StatusCode < 300 && commentRes.Data != null) {
            Comments = commentRes.Data;
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
            RecipeDetailsAction.AddComment => await HandleAddComment(id),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public async Task<IActionResult> HandleAddComment(int id) {
        if (NewComment.Rating is > 5 or < 0) {
            return Page();
        }

        var res = await _recipeService.AddComment(new CreateRecipeCommentDb {
            VersionId = id,
            Rating = NewComment.Rating,
            Comment = NewComment.Comment,
            CreatedAt = DateTime.UtcNow
        });
        if (!string.IsNullOrWhiteSpace(res.ErrorMessage)) {
            Console.WriteLine("Error adding comment: " + res.ErrorMessage);
        }
        return Redirect($"/Recipe/{Id}");
    }

    public async Task<IActionResult> HandleDelete(int id) {
        var res = await _recipeService.DeleteRecipe(id);
        if ((int)res.StatusCode >= 300) {
            Console.WriteLine(res.ErrorMessage);
        }
        return RedirectToPage("Index");
    }
}