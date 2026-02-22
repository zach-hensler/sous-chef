using core.Data;
using core.Models;
using core.Models.DbModels;
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

public class RecipeModel : PageModel {
    public RecipeDetails? Details { get; set; }
    public List<RecipeCommentDb> Comments { get; set; } = [];
    public List<RecipeVersionDb> Versions { get; set; } = [];

    [BindProperty]
    public NewComment NewComment { get; set; } = new() {
        Rating = 0,
        Comment = null
    };

    public int Id { get; set; }

    private async Task LoadPageData(int versionId) {
        var res = await VersionService.GetRecipeByVersion(versionId);
        if ((int)res.StatusCode < 300 && res.Data != null) {
            Details = res.Data;
        }
        else {
            Console.WriteLine(res.ErrorMessage);
            return;
        }

        var commentRes = await RecipeService.GetComments(res.Data.Version.RecipeId);
        if ((int)res.StatusCode < 300 && commentRes.Data != null) {
            Comments = commentRes.Data;
        }
        else {
            Console.WriteLine(res.ErrorMessage);
        }

        var versionRes = await VersionService.List(versionId);
        if ((int)res.StatusCode < 300 && versionRes.Data != null) {
            Versions = versionRes.Data;
        }
        else {
            Console.WriteLine(res.ErrorMessage);
        }
    }
    
    public async Task OnGet(int versionId) {
        Id = versionId;
        await LoadPageData(versionId);
    }

    public async Task<IActionResult> OnPost(int versionId) {
        Id = versionId;
        await LoadPageData(versionId);
        Request.Query.TryGetValue("action", out var action);
        if (!Enum.TryParse<RecipeDetailsAction>(action.ToString(), out var postAction)) {
            return Page();
        }

        return postAction switch {
            RecipeDetailsAction.DeleteConfirmation => await HandleDelete(versionId),
            RecipeDetailsAction.AddComment => await HandleAddComment(versionId),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public async Task<IActionResult> HandleAddComment(int versionId) {
        if (NewComment.Rating is > 5 or < 0) {
            return Page();
        }

        var res = await VersionService.AddComment(new CreateRecipeCommentDb {
            VersionId = versionId,
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
        var res = await VersionService.DeleteRecipeVersion(id);
        if ((int)res.StatusCode >= 300) {
            Console.WriteLine(res.ErrorMessage);
        }
        return RedirectToPage("Index");
    }
}