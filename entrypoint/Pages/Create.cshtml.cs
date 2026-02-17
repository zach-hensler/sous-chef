using System.Net;
using System.Text.Json.Serialization;
using core;
using core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using services;

public record ViewIngedient {
    public required string Name { get; init; }
    public required float Quantity { get; init; }
    public string? Unit { get; init; }

    public CreateRecipeIngredientDb ToIngredientDb() {
        return new CreateRecipeIngredientDb {
            Name = Name,
            Note = "",
            Quantity = Quantity,
            Unit = Unit
        };
    }
}

public record ViewStep {
    public required string Name { get; init; }
    public required string Instruction { get; init; }

    public CreateRecipeStepDb ToStepDb() {
        return new CreateRecipeStepDb {
            Name = Name,
            Instruction = Instruction
        };
    }
}

public record ViewRecipe {
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required EffortLevels EffortLevel { get; init; }
    public required Categories Category { get; init; } = Categories.Uncategorized;
    public required int Time { get; init; }

    public CreateRecipeDb ToRecipeDb() {
        return new CreateRecipeDb {
            Name = Name,
            Description = Description,
            TimeMinutes = Time,
            EffortLevel = EffortLevel,
            Category = Category
        };
    }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CreateActions {
    SaveExisting,
    SaveAsNewMajorVersion,
    SaveAsNewMinorVersion,
    NewIngredient,
    RemoveIngredient,
    NewStep,
    RemoveStep,
    MoveStepDown,
    MoveStepUp
}

public class CreateModel : PageModel {
    [BindProperty] public ViewRecipe RecipeMetadata { get; set; }

    [BindProperty] public List<ViewIngedient> RecipeIngredients { get; set; } = [];

    [BindProperty] public List<ViewStep> RecipeSteps { get; set; } = [];
    public int? VersionId { get; set; }
    [BindProperty] public string? VersionNumber { get; set; }

    public string GetBaseRoute() {
        var baseRoute = "/Create";
        if (VersionId != null) {
            baseRoute += $"/{VersionId}";
        }

        return baseRoute;
    }

    private async Task LoadPageData(int id) {
        var res = await RecipeService.GetRecipe(id);
        if ((int)res.StatusCode >= 300 || res.Data == null) {
            Console.WriteLine(res.ErrorMessage);
        }
        else {
            VersionNumber = res.Data.VersionNumber;
            RecipeMetadata = new ViewRecipe {
                Name = res.Data!.Name,
                Description = res.Data.Description,
                EffortLevel = res.Data.EffortLevel,
                Category = res.Data.Category,
                Time = res.Data.Time
            };
            RecipeIngredients =
                res.Data.Ingredients
                    .Select(i => new ViewIngedient {
                        Name = i.Name,
                        Quantity = i.Quantity,
                        Unit = i.Unit
                    })
                    .ToList();
            RecipeSteps =
                res.Data.Steps
                    .Select(s => new ViewStep {
                        Name = s.Name,
                        Instruction = s.Instruction
                    })
                    .ToList();
        }
    }

    public async Task<IActionResult> OnGet(int? id) {
        if (id != null) {
            VersionId = id;
            await LoadPageData(id.Value);
        }
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id) {
        if (id != null) {
            VersionId = id;
            var versionDetails = await RecipeVersionService.GetVersion(id.Value);
            VersionNumber = versionDetails.Data?.version_number;
        }
        foreach (var kvp in Request.Query) {
            if (!Enum.TryParse<CreateActions>(kvp.Key, out var action)) {
                Console.WriteLine($"Unexpected action: {kvp.Key} = {kvp.Value}");
                return Page();
            }

            return action switch {
                CreateActions.SaveExisting => await HandleSaveExisting(id),
                CreateActions.SaveAsNewMajorVersion => await HandleSaveNewMajorVersion(id),
                CreateActions.SaveAsNewMinorVersion => await HandleSaveNewMinorVersion(id),
                CreateActions.NewIngredient => HandleNewIngredient(),
                CreateActions.NewStep => HandleNewStep(),
                CreateActions.RemoveIngredient => RemoveIngredient(kvp.Value.ToString()),
                CreateActions.RemoveStep => RemoveStep(kvp.Value.ToString()),
                CreateActions.MoveStepDown => MoveStepDown(kvp.Value.ToString()),
                CreateActions.MoveStepUp => MoveStepUp(kvp.Value.ToString()),
                _ => throw new ArgumentOutOfRangeException(kvp.Key)
            };
        }

        return Page();
    }

    private IActionResult MoveStepDown(string idx) {
        if (int.TryParse(idx, out var parsedIdx)) {
            var step = RecipeSteps[parsedIdx];
            RecipeSteps.RemoveAt(parsedIdx);
            RecipeSteps.Insert(parsedIdx + 1, step);
        }
        return Page();
    }

    private IActionResult MoveStepUp(string idx) {
        if (int.TryParse(idx, out var parsedIdx)) {
            var step = RecipeSteps[parsedIdx];
            RecipeSteps.RemoveAt(parsedIdx);
            RecipeSteps.Insert(parsedIdx - 1, step);
        }
        return Page();
    }

    private IActionResult RemoveStep(string idx) {
        if (int.TryParse(idx, out var parsedIdx)) {
            RecipeSteps.RemoveAt(parsedIdx);
        }
        return Page();
    }
    private IActionResult RemoveIngredient(string idx) {
        if (int.TryParse(idx, out var parsedIdx)) {
            RecipeIngredients.RemoveAt(parsedIdx);
        }
        return Page();
    }
    private IActionResult HandleNewStep() {
        RecipeSteps.Add(new ViewStep {
            Name = "",
            Instruction = ""
        });
        return Page();
    }

    private IActionResult HandleNewIngredient() {
        RecipeIngredients.Add(new ViewIngedient {
            Name = "",
            Quantity = 0f,
            Unit = ""
        });
        return Page();
    }

    private async Task<IActionResult> HandleSaveNewMajorVersion(int? versionId) {
        if (versionId == null) {
            return Page();
        }

        var res = await RecipeService.CreateRecipeVersion(new CreateRecipeVersionRequest {
            PreviousVersionId = versionId.Value,
            VersionType = VersionType.Major,
            Recipe = RecipeMetadata.ToRecipeDb(),
            Steps = RecipeSteps.Select(s => s.ToStepDb()).ToList(),
            Ingredients = RecipeIngredients.Select(i => i.ToIngredientDb()).ToList()
        });
        if ((int)res.StatusCode < 300) {
            return Redirect("/Index");
        }

        Console.WriteLine($"Unable to create new version, received error '{res.ErrorMessage}'");
        return Page();
    }
    
    private async Task<IActionResult> HandleSaveNewMinorVersion(int? versionId) {
        if (versionId == null) {
            return Page();
        }

        var res = await RecipeService.CreateRecipeVersion(new CreateRecipeVersionRequest {
            PreviousVersionId = versionId.Value,
            VersionType = VersionType.Minor,
            Recipe = RecipeMetadata.ToRecipeDb(),
            Steps = RecipeSteps.Select(s => s.ToStepDb()).ToList(),
            Ingredients = RecipeIngredients.Select(i => i.ToIngredientDb()).ToList()
        });
        if ((int)res.StatusCode < 300) {
            return Redirect("/Index");
        }

        Console.WriteLine($"Unable to create new version, received error '{res.ErrorMessage}'");
        return Page();
    }

    private async Task<IActionResult> HandleSaveExisting(int? versionId) {
        var createRequest = new CreateRecipeRequest {
            Recipe = RecipeMetadata.ToRecipeDb(),
            Steps = RecipeSteps.Select(s => s.ToStepDb()).ToList(),
            Ingredients = RecipeIngredients.Select(i => i.ToIngredientDb()).ToList()
        };

        Response? response;
        int redirectId;
        if (versionId == null) {
            var createRes = await RecipeService.CreateRecipe(createRequest);
            redirectId = createRes.Data;
            response = createRes;
        }
        else {
            response = await RecipeService.UpdateRecipeVersion(versionId.Value, createRequest);
            // redirectId = (int)versionId;
        }

        if (response.StatusCode == HttpStatusCode.OK) {
            return Redirect("/Index");
        }

        Console.WriteLine(response.ErrorMessage);
        return Page();
    }
}