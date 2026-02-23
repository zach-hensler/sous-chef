using System.Net;
using System.Text.Json.Serialization;
using core.Models;
using core.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using services;

namespace sous_chef.Pages;

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
    public string? VersionNumber { get; set; }

    public string GetBaseRoute() {
        var baseRoute = "/Create";
        if (VersionId != null) {
            baseRoute += $"/{VersionId}";
        }

        return baseRoute;
    }

    private async Task LoadPageData(int versionId) {
        var res = await VersionService.GetRecipeByVersion(versionId);
        if ((int)res.StatusCode >= 300 || res.Data == null) {
            Console.WriteLine(res.ErrorMessage);
        }
        else {
            RecipeMetadata = res.Data.RecipeMetadata.ToViewRecipe();
            VersionNumber = res.Data.Version.VersionNumber;
            RecipeIngredients = res.Data.Ingredients.Select(i => i.ToViewIngredient()).ToList();
            RecipeSteps = res.Data.Steps.Select(s => s.ToViewStep()).ToList();
        }
    }

    public async Task<IActionResult> OnGet(int? versionId) {
        if (versionId != null) {
            VersionId = versionId;
            await LoadPageData(versionId.Value);
        }
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? versionId) {
        if (versionId != null) {
            VersionId = versionId;
        }
        foreach (var kvp in Request.Query) {
            if (!Enum.TryParse<CreateActions>(kvp.Key, out var action)) {
                Console.WriteLine($"Unexpected action: {kvp.Key} = {kvp.Value}");
                return Page();
            }

            return action switch {
                CreateActions.SaveExisting => await HandleSaveExisting(versionId),
                CreateActions.SaveAsNewMajorVersion => await HandleSaveNewMajorVersion(versionId),
                CreateActions.SaveAsNewMinorVersion => await HandleSaveNewMinorVersion(versionId),
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

    private void SyncStepAttemptedValues() {
        // TextAreas don't automatically sync and must be manually synced
        for (var i = 0; i < RecipeSteps.Count; i++) {
            ModelState[$"{nameof(RecipeSteps)}[{i}].{nameof(ViewStep.Name)}"]?.AttemptedValue = RecipeSteps[i].Name;
            ModelState[$"{nameof(RecipeSteps)}[{i}].{nameof(ViewStep.Instruction)}"]?.AttemptedValue = RecipeSteps[i].Instruction;
        }
    }

    private IActionResult MoveStepDown(string idx) {
        if (int.TryParse(idx, out var parsedIdx)) {
            var step = RecipeSteps[parsedIdx];
            RecipeSteps.RemoveAt(parsedIdx);
            RecipeSteps.Insert(parsedIdx + 1, step);
        }

        SyncStepAttemptedValues();
        return Page();
    }

    private IActionResult MoveStepUp(string idx) {
        if (int.TryParse(idx, out var parsedIdx)) {
            var step = RecipeSteps[parsedIdx];
            RecipeSteps.RemoveAt(parsedIdx);
            RecipeSteps.Insert(parsedIdx - 1, step);
        }

        SyncStepAttemptedValues();
        return Page();
    }

    private IActionResult RemoveStep(string idx) {
        if (int.TryParse(idx, out var parsedIdx)) {
            RecipeSteps.RemoveAt(parsedIdx);
        }

        SyncStepAttemptedValues();
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
            redirectId = createRes.Data.VersionId;
            response = createRes;
        }
        else {
            response = await RecipeService.UpdateRecipeVersion(versionId.Value, createRequest);
            redirectId = versionId.Value;
        }

        if (response.StatusCode == HttpStatusCode.OK) {
            return Redirect($"/Recipe/{redirectId}");
        }

        Console.WriteLine(response.ErrorMessage);
        return Page();
    }
}