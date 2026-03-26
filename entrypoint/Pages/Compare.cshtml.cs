using core.Models.DbModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using services;

namespace sous_chef.Pages;

public class Compare : PageModel {
    public VersionId? VersionId1 { get; set; }
    public VersionId? VersionId2 { get; set; }
    
    public string RecipeName { get; set; }
    public string Version1Number { get; set; }
    public string Version2Number { get; set; }

    public record Comparison {
        public enum ComparisonType {
            Added,
            Deleted,
            Updated,
            Same
        }
        public required ComparisonType Type { get; init; }
        public required string? Item1 { get; init; }
        public required string? Item2 { get; init; }
    }
    public List<Comparison> StepComparisons { get; set; } = [];
    public List<Comparison> IngredientComparisons { get; set; } = [];
    
    public List<RecipeVersionDb>? Versions { get; set; }

    public async Task<IActionResult> OnGet(int? id1, int? id2) {
        if (id1 == null || id2 == null) {
            return Page();
        }

        VersionId1 = new VersionId(id1.Value);
        var v1Res = await RecipeService.GetRecipeByVersion(VersionId1);
        if (v1Res.Data == null) {
            return Page();
        }

        RecipeName = v1Res.Data!.RecipeMetadata.Name;
        Version1Number = v1Res.Data.Version.VersionNumber;

        VersionId2 = new VersionId(id2.Value);
        var v2Res = await RecipeService.GetRecipeByVersion(VersionId2);
        if (v2Res.Data == null) {
            return Page();
        }

        Version2Number = v2Res.Data.Version.VersionNumber;

        var listRes = await VersionService.List(VersionId1);
        if (listRes.Data != null) {
            Versions = listRes.Data;
        }

        var v1Ingredients = v1Res.Data.Ingredients.OrderBy(i => i.Name).ToList();
        var v2Ingredients = v2Res.Data.Ingredients.OrderBy(i => i.Name).ToList();
        foreach (var v1Ingredient in v1Ingredients) {
            var v2Ingredient = v2Ingredients.FirstOrDefault(i => i.Name == v1Ingredient.Name);
            Comparison.ComparisonType type;
            // TODO check for diff
            if (v2Ingredient == null) {
                type = Comparison.ComparisonType.Deleted;
            }
            else {
                type = Comparison.ComparisonType.Same;
            }
            IngredientComparisons.Add(
                new Comparison {
                    Type = type,
                    Item1 = v1Ingredient.Name,
                    Item2 = v2Ingredient?.Name
                });
        }
        IngredientComparisons.AddRange(
            v2Ingredients
                .Where(i2 => v1Ingredients.Find(i1 => i1.Name == i2.Name) == null)
                .Select(i => new Comparison {
                    Type = Comparison.ComparisonType.Added,
                    Item1 = null,
                    Item2 = i.Name
                }));

        var v1Steps = v1Res.Data.Steps.OrderBy(s => s.StepNumber).ToList();
        var v2Steps = v2Res.Data.Steps.OrderBy(s => s.StepNumber).ToList();
        foreach (var step1 in v1Steps) {
            var step2 = v2Steps.FirstOrDefault(s => s.StepNumber == step1.StepNumber);
            Comparison.ComparisonType type;
            // TODO check for diff
            if (step2 == null) {
                type = Comparison.ComparisonType.Deleted;
            }
            else {
                type = Comparison.ComparisonType.Same;
            }
            StepComparisons.Add(
                new Comparison {
                    Type = type,
                    Item1 = $"{step1.StepNumber}. {step1.Name}",
                    Item2 = step2 != null ? $"{step2.StepNumber}. {step2.Name}" : null
                });
        }
        StepComparisons.AddRange(
            v2Steps
                .Where(s2 => v1Steps.Find(s1 => s1.StepNumber == s2.StepNumber) == null)
                .Select(s => new Comparison {
                    Type = Comparison.ComparisonType.Added,
                    Item1 = null,
                    Item2 = $"{s.StepNumber}. {s.Name}"
                }));
        
        return Page();
    }
}