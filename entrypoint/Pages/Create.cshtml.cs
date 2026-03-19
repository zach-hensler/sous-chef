using System.Net;
using System.Text.Json.Serialization;
using core;
using core.Models;
using core.Models.DbModels;
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
    [BindProperty] public string AutoFocusId { get; set; } = "";
    [BindProperty] public ViewRecipe RecipeMetadata { get; set; }

    [BindProperty] public List<ViewIngedient> RecipeIngredients { get; set; } = [];

    [BindProperty] public List<ViewStep> RecipeSteps { get; set; } = [];
    public VersionId? VersionId { get; set; }
    public string? VersionNumber { get; set; }

    public string GetBaseRoute() {
        var baseRoute = "/Create";
        if (VersionId != null) {
            baseRoute += $"/{VersionId}";
        }

        return baseRoute;
    }

    private async Task LoadPageData(VersionId versionId) {
        var res = await VersionService.GetRecipeByVersion(versionId);
        if (res.Data != null) {
            RecipeMetadata = res.Data.RecipeMetadata.ToViewRecipe();
            VersionNumber = res.Data.Version.VersionNumber;
            RecipeIngredients = res.Data.Ingredients.Select(i => i.ToViewIngredient()).ToList();
            RecipeSteps = res.Data.Steps.Select(s => s.ToViewStep()).ToList();
        }
    }

    public async Task<IActionResult> OnGet(int? versionId) {
        if (versionId != null) {
            VersionId = new VersionId(versionId.Value);
            await LoadPageData(VersionId);
        }
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? versionId) {
        if (versionId != null) {
            VersionId = new VersionId(versionId.Value);
        }
        foreach (var kvp in Request.Query) {
            if (!Enum.TryParse<CreateActions>(kvp.Key, out var action)) {
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
            AutoFocusId = $"step-name-{parsedIdx + 1}";
        }

        SyncStepAttemptedValues();
        return Page();
    }

    private IActionResult MoveStepUp(string idx) {
        if (int.TryParse(idx, out var parsedIdx)) {
            var step = RecipeSteps[parsedIdx];
            RecipeSteps.RemoveAt(parsedIdx);
            RecipeSteps.Insert(parsedIdx - 1, step);
            AutoFocusId = $"step-name-{parsedIdx - 1}";
        }

        SyncStepAttemptedValues();
        return Page();
    }

    private IActionResult RemoveStep(string idx) {
        if (int.TryParse(idx, out var parsedIdx)) {
            RecipeSteps.RemoveAt(parsedIdx);
            AutoFocusId = $"step-name-{Math.Clamp(parsedIdx - 1, 0, RecipeSteps.Count)}";
        }

        SyncStepAttemptedValues();
        return Page();
    }
    private IActionResult RemoveIngredient(string idx) {
        if (int.TryParse(idx, out var parsedIdx)) {
            RecipeIngredients.RemoveAt(parsedIdx);
            AutoFocusId = $"ingredient-name-{Math.Clamp(parsedIdx - 1, 0, RecipeIngredients.Count)}";
        }
        return Page();
    }
    private IActionResult HandleNewStep() {
        RecipeSteps.Add(new ViewStep {
            Name = "",
            Instruction = ""
        });
        AutoFocusId = $"step-name-{RecipeSteps.Count - 1}";
        return Page();
    }

    private IActionResult HandleNewIngredient() {
        RecipeIngredients.Add(new ViewIngedient {
            Name = "",
            Quantity = 0f,
            Unit = ""
        });
        AutoFocusId = $"ingredient-name-{RecipeIngredients.Count - 1}";
        return Page();
    }

    private async Task<IActionResult> HandleSaveNewMajorVersion(int? versionId) {
        if (versionId == null) {
            return Page();
        }

        VersionId = new VersionId(versionId.Value);
        var res = await RecipeService.CreateRecipeVersion(new CreateRecipeVersionRequest {
            PreviousVersionId = VersionId,
            VersionType = VersionType.Major,
            Recipe = RecipeMetadata.ToRecipeDb(),
            Steps = RecipeSteps.Select(s => s.ToStepDb()).ToList(),
            Ingredients = RecipeIngredients.Select(i => i.ToIngredientDb()).ToList()
        });
        if ((int)res.StatusCode < 300) {
            return Redirect("/Index");
        }

        return Page();
    }
    
    private async Task<IActionResult> HandleSaveNewMinorVersion(int? versionId) {
        if (versionId == null) {
            return Page();
        }

        VersionId = new VersionId(versionId.Value);
        var res = await RecipeService.CreateRecipeVersion(new CreateRecipeVersionRequest {
            PreviousVersionId = VersionId,
            VersionType = VersionType.Minor,
            Recipe = RecipeMetadata.ToRecipeDb(),
            Steps = RecipeSteps.Select(s => s.ToStepDb()).ToList(),
            Ingredients = RecipeIngredients.Select(i => i.ToIngredientDb()).ToList()
        });
        if ((int)res.StatusCode < 300) {
            return Redirect("/Index");
        }

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
            if (!string.IsNullOrWhiteSpace(createRes.ErrorMessage)) {
                return Page();
            }
            redirectId = createRes.Data!.VersionId.Value;
            response = createRes;
        }
        else {
            VersionId = new VersionId(versionId.Value);
            response = await RecipeService.UpdateRecipeVersion(VersionId, createRequest);
            redirectId = versionId.Value;
        }

        if (response.StatusCode == HttpStatusCode.OK) {
            return Redirect($"/Recipe/{redirectId}");
        }

        return Page();
    }
}