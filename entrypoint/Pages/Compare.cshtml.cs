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
        public required string? Item1Line1 { get; init; }
        public required string? Item1Line2 { get; init; }
        public required string? Item2Line1 { get; init; }
        public required string? Item2Line2 { get; init; }
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
            var item1 = $"{v1Ingredient.Name} {v1Ingredient.Quantity} {v1Ingredient.Unit}";
            var item2 = v2Ingredient == null ? null : $"{v2Ingredient.Name} {v2Ingredient.Quantity} {v2Ingredient.Unit}";

            Comparison.ComparisonType type;
            if (v2Ingredient == null) {
                type = Comparison.ComparisonType.Deleted;
            }
            else if (item1 != item2) {
                type = Comparison.ComparisonType.Updated;
            }
            else {
                type = Comparison.ComparisonType.Same;
            }
            IngredientComparisons.Add(
                new Comparison {
                    Type = type,
                    Item1Line1 = item1,
                    Item2Line1 = item2,
                    Item1Line2 = null,
                    Item2Line2 = null
                });
        }
        IngredientComparisons.AddRange(
            v2Ingredients
                .Where(i2 => v1Ingredients.Find(i1 => i1.Name == i2.Name) == null)
                .Select(i => new Comparison {
                    Type = Comparison.ComparisonType.Added,
                    Item1Line1 = null,
                    Item2Line1 = i.Name,
                    Item1Line2 = null,
                    Item2Line2 = null
                }));

        var v1Steps = v1Res.Data.Steps.OrderBy(s => s.StepNumber).ToList();
        var v2Steps = v2Res.Data.Steps.OrderBy(s => s.StepNumber).ToList();
        foreach (var step1 in v1Steps) {
            var step2 = v2Steps.FirstOrDefault(s => s.Name == step1.Name);
            Comparison.ComparisonType type;
            if (step2 == null) {
                type = Comparison.ComparisonType.Deleted;
            }
            else if (step1.StepNumber+step1.Name+step1.Instruction != step2.StepNumber+step2.Name+step2.Instruction) {
                type = Comparison.ComparisonType.Updated;
            }
            else {
                type = Comparison.ComparisonType.Same;
            }
            StepComparisons.Add(
                new Comparison {
                    Type = type,
                    Item1Line1 = $"{step1.StepNumber}. {step1.Name}",
                    Item2Line1 = step2 != null
                        ? $"{step2.StepNumber}. {step2.Name}"
                        : null,
                    Item1Line2 = step1?.Instruction,
                    Item2Line2 = step2?.Instruction
                });
        }
        StepComparisons.AddRange(
            v2Steps
                .Where(s2 => v1Steps.Find(s1 => s1.Name == s2.Name) == null)
                .Select(s => new Comparison {
                    Type = Comparison.ComparisonType.Added,
                    Item1Line1 = null,
                    Item2Line1 = $"{s.StepNumber}. {s.Name}",
                    Item1Line2 = null,
                    Item2Line2 = s.Instruction
                }));
        
        return Page();
    }
}