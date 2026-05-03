using core.Data;
using core.Models.DbModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using services;

namespace sous_chef.Pages;

public enum RecipeDetailsAction {
    AddComment,
    DeleteVersion,
    DeleteEntireRecipe
}

public record NewComment {
    public required int? Rating { get; init; }
    public required string? Comment { get; init; }
}

public class RecipeModel : PageModel {
    public RecipeDetails? Details { get; set; }
    public List<RecipeCommentDb> Comments { get; set; } = [];
    public List<RecipeVersionDb> Versions { get; set; } = [];
    [BindProperty] public string DeleteConfirmation { get; set; } = "";

    [BindProperty]
    public NewComment NewComment { get; set; } = new() {
        Rating = null,
        Comment = null
    };

    public VersionId? Id { get; set; }

    private async Task LoadPageData(VersionId versionId) {
        Details = await VersionService.GetRecipeByVersion(versionId);
        if (Details == null) {
            return;
        }
        Comments = await RecipeService.GetComments(Details.Version.RecipeId);
        Versions = await VersionService.List(versionId);
    }
    
    public async Task OnGet(int versionId) {
        Id = new VersionId(versionId);
        await LoadPageData(Id);
    }

    public async Task<IActionResult> OnPost(int versionId) {
        Id = new VersionId(versionId);
        await LoadPageData(Id);
        Request.Query.TryGetValue("action", out var action);
        if (!Enum.TryParse<RecipeDetailsAction>(action.ToString(), out var postAction)) {
            return Page();
        }

        return postAction switch {
            RecipeDetailsAction.AddComment => await HandleAddComment(versionId),
            RecipeDetailsAction.DeleteVersion => await HandleDeleteVersion(versionId),
            RecipeDetailsAction.DeleteEntireRecipe => await HandleDeleteRecipe(versionId),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public async Task<IActionResult> HandleAddComment(int versionId) {
        Id = new VersionId(versionId);
        if (NewComment.Rating is null or > 5 or < 0) {
            return Page();
        }

        await VersionService.AddComment(new CreateRecipeCommentDb {
            VersionId = Id,
            Rating = NewComment.Rating.Value,
            Comment = NewComment.Comment,
            CreatedAt = DateTime.UtcNow
        });
        return Redirect($"/Recipe/{Id}#comments");
    }

    public async Task<IActionResult> HandleDeleteVersion(int versionId) {
        if (DeleteConfirmation != "DELETE") {
            return Page();
        }
        Id = new VersionId(versionId);
        await VersionService.DeleteRecipeVersion(Id);
        return RedirectToPage("Index");
    }

    public async Task<IActionResult> HandleDeleteRecipe(int versionId) {
        if (DeleteConfirmation != "DELETE") {
            return Page();
        }
        Id = new VersionId(versionId);
        await VersionService.DeleteEntireRecipe(Id);
        return RedirectToPage("Index");
    }
}