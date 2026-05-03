using core.Models.DbModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using services;

namespace sous_chef.Pages;

public class Compare : PageModel {
    public VersionId? VersionId1 { get; set; }
    public VersionId? VersionId2 { get; set; }

    public string RecipeName { get; set; } = "";

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
    public List<Comparison> CommentComparisons { get; set; } = [];
    
    public List<RecipeVersionDb>? Versions { get; set; }

    public async Task<IActionResult> OnGet(int? id1, int? id2) {
        if (id1 == null || id2 == null) {
            return Page();
        }

        VersionId1 = new VersionId(id1.Value);
        var v1Res = await RecipeService.GetRecipeByVersion(VersionId1);
        if (v1Res == null) {
            return Page();
        }

        RecipeName = v1Res.RecipeMetadata.Name;

        VersionId2 = new VersionId(id2.Value);
        var v2Res = await RecipeService.GetRecipeByVersion(VersionId2);
        if (v2Res == null) {
            return Page();
        }

        Versions = await VersionService.List(VersionId1);

        var v1Ingredients = v1Res.Ingredients.OrderBy(i => i.Name).ToList();
        var v2Ingredients = v2Res.Ingredients.OrderBy(i => i.Name).ToList();
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
                    Item2Line1 = $"{i.Name} {i.Quantity} {i.Unit}",
                    Item1Line2 = null,
                    Item2Line2 = null
                }));

        var v1Steps = v1Res.Steps.OrderBy(s => s.StepNumber).ToList();
        var v2Steps = v2Res.Steps.OrderBy(s => s.StepNumber).ToList();
        var v1Counter = 0;
        var v2Counter = 0;
        for (var i = 0; i < Math.Max(v1Steps.Count, v2Steps.Count); i++) {
            var v1Step = v1Steps.ElementAtOrDefault(v1Counter);
            var v2Step = v2Steps.ElementAtOrDefault(v2Counter);
            if (v1Step == null) {
                StepComparisons.Add(
                    new Comparison {
                        Type = Comparison.ComparisonType.Added,
                        Item1Line1 = null,
                        Item1Line2 = null,
                        Item2Line1 = $"{v2Step!.StepNumber}. {v2Step.Name}",
                        Item2Line2 = v2Step.Instruction
                    });
                v2Counter++;
                continue;
            }
            if (v2Step == null) {
                StepComparisons.Add(
                    new Comparison {
                        Type = Comparison.ComparisonType.Deleted,
                        Item1Line1 = null,
                        Item1Line2 = null,
                        Item2Line1 = $"{v1Step.StepNumber}. {v1Step.Name}",
                        Item2Line2 = v1Step.Instruction
                    });
                v1Counter++;
                continue;
            }
            if (v1Step.Name == v2Step.Name) {
                StepComparisons.Add(
                    new Comparison {
                        Type = v1Step.Instruction == v2Step.Instruction
                            ? Comparison.ComparisonType.Same
                            : Comparison.ComparisonType.Updated,
                        Item1Line1 = $"{v1Step.StepNumber}. {v1Step.Name}",
                        Item1Line2 = v1Step.Instruction,
                        Item2Line1 = $"{v2Step.StepNumber}. {v2Step.Name}",
                        Item2Line2 = v2Step.Instruction
                    });
                v1Counter++;
                v2Counter++;
                continue;
            }
            
            var v1Next = v1Steps.ElementAtOrDefault(v1Counter + 1);
            var v2Next = v2Steps.ElementAtOrDefault(v2Counter + 1);
            if (v1Step.Name == v2Next?.Name) {
                StepComparisons.Add(
                    new Comparison {
                        Type = Comparison.ComparisonType.Added,
                        Item1Line1 = null,
                        Item1Line2 = null,
                        Item2Line1 = $"{v2Step.StepNumber}. {v2Step.Name}",
                        Item2Line2 = v2Step.Instruction
                    });
                v2Counter++;
                continue;
            }
            if (v2Step.Name == v1Next?.Name) {
                StepComparisons.Add(
                    new Comparison {
                        Type = Comparison.ComparisonType.Deleted,
                        Item1Line1 = $"{v1Step.StepNumber}. {v1Step.Name}",
                        Item1Line2 = v1Step.Instruction,
                        Item2Line1 = null,
                        Item2Line2 = null
                    });
                v1Counter++;
                continue;
            }
            StepComparisons.Add(
                new Comparison {
                    Type = Comparison.ComparisonType.Added,
                    Item1Line1 = null,
                    Item1Line2 = null,
                    Item2Line1 = $"{v2Step.StepNumber}. {v2Step.Name}",
                    Item2Line2 = v2Step.Instruction
                });
            StepComparisons.Add(
                new Comparison {
                    Type = Comparison.ComparisonType.Deleted,
                    Item1Line1 = $"{v1Step.StepNumber}. {v1Step.Name}",
                    Item1Line2 = v1Step.Instruction,
                    Item2Line1 = null,
                    Item2Line2 = null
                });
            v1Counter++;
            v2Counter++;
        }

        var v1Comments =
            v1Res.Comments.OrderByDescending(c => c.Rating).ToList();
        var v2Comments =
            v2Res.Comments.OrderByDescending(c => c.Rating).ToList();
        for (var i = 0; i < Math.Max(v1Comments.Count, v2Comments.Count); i++) {
            var v1Comment = v1Comments.ElementAtOrDefault(i);
            var v2Comment = v2Comments.ElementAtOrDefault(i);
            CommentComparisons.Add(new Comparison {
                Type = Comparison.ComparisonType.Same,
                Item1Line1 = Format(v1Comment),
                Item1Line2 = v1Comment?.Comment,
                Item2Line1 = Format(v2Comment),
                Item2Line2 = v2Comment?.Comment
            });

            continue;

            string? Format(RecipeCommentDb? comment) =>
                comment == null ? null : $"Rating: {comment.Rating}/5 at {comment.CreatedAt}";
        }

        return Page();
    }
}