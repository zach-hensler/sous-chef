using core.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using services;

namespace sous_chef.Pages;

public class Compare : PageModel {
    public int? VersionId1 { get; set; }
    public int? VersionId2 { get; set; }
    
    public RecipeDetails? Version1Details { get; set; }
    public RecipeDetails? Version2Details { get; set; }

    public async Task<IActionResult> OnGet(int? id1, int? id2) {
        if (id1 != null) {
            VersionId1 = id1;
            var res = await RecipeService.GetRecipeByVersion(id1.Value);
            if (!string.IsNullOrWhiteSpace(res.ErrorMessage)) {
                Version1Details = res.Data;
            }
            else {
                Console.WriteLine(res.ErrorMessage);
            }
        }

        if (id2 != null) {
            VersionId2 = id2;
            var res = await RecipeService.GetRecipeByVersion(id2.Value);
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