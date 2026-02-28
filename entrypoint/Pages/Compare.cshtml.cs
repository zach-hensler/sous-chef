using core.Data;
using core.Models.DbModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using services;

namespace sous_chef.Pages;

public class Compare : PageModel {
    public VersionId? VersionId1 { get; set; }
    public VersionId? VersionId2 { get; set; }
    
    public RecipeDetails? Version1Details { get; set; }
    public RecipeDetails? Version2Details { get; set; }

    public async Task<IActionResult> OnGet(int? id1, int? id2) {
        if (id1 != null) {
            VersionId1 = new VersionId(id1.Value);
            var res = await RecipeService.GetRecipeByVersion(VersionId1);
            if (!string.IsNullOrWhiteSpace(res.ErrorMessage)) {
                Version1Details = res.Data;
            }
            else {
                Console.WriteLine(res.ErrorMessage);
            }
        }

        if (id2 != null) {
            VersionId2 = new VersionId(id2.Value);
            var res = await RecipeService.GetRecipeByVersion(VersionId2);
            if (!string.IsNullOrWhiteSpace(res.ErrorMessage)) {
                Version2Details = res.Data;
            }
            else {
                Console.WriteLine(res.ErrorMessage);
            }
        }
        return Page();
    }
}