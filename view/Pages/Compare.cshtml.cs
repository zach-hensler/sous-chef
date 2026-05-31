using core.Models.DbModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using services;

namespace view.Pages;

public static class CompareHelpers {
    public enum ComparisonType {
        Added,
        Deleted,
        Updated,
        Same
    }
    public record Comparison {
        public required ComparisonType Type { get; init; }
        public required string? Item1Line1 { get; init; }
        public required string? Item1Line2 { get; init; }
        public required string? Item2Line1 { get; init; }
        public required string? Item2Line2 { get; init; }
    }

    // For comparisons where order does not need to be preserved
    public static List<Comparison> CompareUnordered<T>(
        List<T> oldList,
        List<T> newList,
        Func<T?, T?, bool> matches,
        Func<T?, T?, bool> equals,
        Func<T?, string?> lineOneFormatter,
        Func<T?, string?> lineTwoFormatter) where T : class {
        List<Comparison> result = [];
        result.AddRange(from newItem in newList
            let match = oldList.FirstOrDefault(old => matches(old, newItem))
            let r = oldList.Remove(match)
            let matchEquals = @equals(match, newItem)
            let type = (match, matchEquals) switch {
                (null, _) => ComparisonType.Added,
                (_, true) => ComparisonType.Same,
                (_, false) => ComparisonType.Updated
            }
            select new Comparison {
                Type = type,
                Item1Line1 = lineOneFormatter(match),
                Item1Line2 = lineTwoFormatter(match),
                Item2Line1 = lineOneFormatter(newItem),
                Item2Line2 = lineTwoFormatter(newItem)
            });

        // add any remaining items in oldList
        result.AddRange(
            oldList.Select(remaining => new Comparison {
                Type = ComparisonType.Deleted,
                Item1Line1 = lineOneFormatter(remaining),
                Item1Line2 = lineTwoFormatter(remaining),
                Item2Line1 = null,
                Item2Line2 = null
            }));

        return result;
    }

    // For comparisons where order needs to be preserved
    public static List<Comparison> CompareOrdered<T>(
        List<T> oldList,
        List<T> newList,
        Func<T?, T?, bool> matches,
        Func<T?, T?, bool> equals,
        Func<T?, string?> lineOneFormatter,
        Func<T?, string?> lineTwoFormatter) where T : class {
        var comparisons = new List<Comparison>();
        var oldCounter = 0;
        var newCounter = 0;

        for (var i = 0; i < Math.Max(oldList.Count, newList.Count); i++) {
            var oldStep = oldList.ElementAtOrDefault(oldCounter);
            var newStep = newList.ElementAtOrDefault(newCounter);

            // At end of list
            if (oldStep == null) {
                comparisons.Add(Format(null, newStep, ComparisonType.Added));
                newCounter++;
                continue;
            }

            // At end of list
            if (newStep == null) {
                comparisons.Add(Format(oldStep, null, ComparisonType.Deleted));
                oldCounter++;
                continue;
            }

            if (equals(oldStep, newStep)) {
                // TODO handle "updated" when things are modified
                comparisons.Add(Format(oldStep, newStep, ComparisonType.Same));
                oldCounter++;
                newCounter++;
                continue;
            }

            var oldNext = oldList.ElementAtOrDefault(oldCounter + 1);
            var newNext = newList.ElementAtOrDefault(newCounter + 1);
            if (equals(oldStep, newNext)) {
                comparisons.Add(Format(null, newStep, ComparisonType.Added));
                newCounter++;
                continue;
            }

            if (equals(newStep, oldNext)) {
                comparisons.Add(Format(oldStep, null, ComparisonType.Deleted));
                oldCounter++;
                continue;
            }


            comparisons.Add(Format(null, newStep, ComparisonType.Added));
            comparisons.Add(Format(oldStep, null, ComparisonType.Deleted));
            oldCounter++;
            newCounter++;
        }

        return comparisons;

        Comparison Format(T? item1, T? item2, ComparisonType type) => new() {
            Type = type,
            Item1Line1 = lineOneFormatter(item1),
            Item1Line2 = lineTwoFormatter(item1),
            Item2Line1 = lineOneFormatter(item2),
            Item2Line2 = lineTwoFormatter(item2)
        };
    }
}

public class CompareModel : PageModel {
    public VersionId? VersionId1 { get; set; }
    public VersionId? VersionId2 { get; set; }

    public string RecipeName { get; set; } = "";


    public List<CompareHelpers.Comparison> StepComparisons { get; set; } = [];
    public List<CompareHelpers.Comparison> IngredientComparisons { get; set; } = [];
    public List<CompareHelpers.Comparison> CommentComparisons { get; set; } = [];

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

        string? FormatIngredient(RecipeIngredientDb? i) => i == null ? null : $"{i.Name} {i.Quantity} {i.Unit}";
        IngredientComparisons = CompareHelpers.CompareUnordered(
            v1Res.Ingredients.OrderBy(i => i.Name).ToList(),
            v2Res.Ingredients.OrderBy(i => i.Name).ToList(),
            (i1, i2) => i1?.Name == i2?.Name,
            (i1, i2) => i1?.Quantity - i2?.Quantity <= 0.01,
            FormatIngredient,
            _ => null);

        StepComparisons = CompareHelpers.CompareOrdered(
            v1Res.Steps.OrderBy(s => s.StepNumber).ToList(),
            v2Res.Steps.OrderBy(s => s.StepNumber).ToList(),
            (s1, s2) => s1?.Name == s2?.Name,
            (s1, s2) => s1?.Instruction == s2?.Instruction,
            s => s == null ? null : $"{s.StepNumber}. {s.Name}",
            s => s?.Instruction);

        CommentComparisons = CompareHelpers.CompareUnordered(
            v1Res.Comments.OrderByDescending(c => c.Rating).ToList(),
            v2Res.Comments.OrderByDescending(c => c.Rating).ToList(),
            (_, _) => true,
            (_, _) => true,
            c => c == null
                ? null
                : $"Rating: {c.Rating}/5 @ {ViewUtils.FormatDate(c.CreatedAt)}",
            c => c?.Comment);

        return Page();
    }
}