using System.Text.Json.Serialization;
using core.Models;
using core.Models.DbModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using services;

namespace sous_chef.Pages;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CreateActions {
    SaveExisting,
    SaveAsNewVersion,
    NewIngredient,
    RemoveIngredient,
    NewStep,
    RemoveStep,
    MoveStepDown,
    MoveStepUp
}

// TODO convert this into the create db models
public class CreateModel : PageModel {
    [BindProperty] public string AutoFocusId { get; set; } = "";
    [BindProperty] public string UpdateMessage { get; set; } = "";
    [BindProperty] public CreateRecipeView? RecipeMetadata { get; set; }

    [BindProperty] public List<ViewIngedient> RecipeIngredients { get; set; } = [];

    [BindProperty] public List<CreateStepView> RecipeSteps { get; set; } = [];
    public VersionId? VersionId { get; set; }

    public string GetBaseRoute() {
        var baseRoute = "/Create";
        if (VersionId != null) {
            baseRoute += $"/{VersionId}";
        }

        return baseRoute;
    }

    private async Task LoadPageData(VersionId versionId) {
        var res = await VersionService.GetRecipeByVersion(versionId);
        if (res != null) {
            RecipeMetadata = res.RecipeMetadata.ToViewRecipe();
            RecipeIngredients = res.Ingredients.Select(i => i.ToViewIngredient()).ToList();
            RecipeSteps = res.Steps.Select(s => s.ToViewStep()).ToList();
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
                CreateActions.SaveAsNewVersion => await HandleSaveNewVersion(versionId),
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
            ModelState[$"{nameof(RecipeSteps)}[{i}].{nameof(CreateStepView.Name)}"]?.AttemptedValue = RecipeSteps[i].Name;
            ModelState[$"{nameof(RecipeSteps)}[{i}].{nameof(CreateStepView.Instruction)}"]?.AttemptedValue = RecipeSteps[i].Instruction;
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
        RecipeSteps.Add(new CreateStepView {
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

    private async Task<IActionResult> HandleSaveNewVersion(int? versionId) {
        if (versionId == null || RecipeMetadata == null) {
            return Page();
        }

        VersionId = new VersionId(versionId.Value);
        var res = await RecipeService.CreateRecipeVersion(new CreateRecipeVersionRequest {
            PreviousVersionId = VersionId,
            Recipe = RecipeMetadata.ToRecipeDb(),
            Steps = RecipeSteps.Select(s => s.ToStepDb())
                .ToList(),
            Ingredients = RecipeIngredients.Select(i => i.ToIngredientDb())
                .ToList(),
            Message = UpdateMessage
        });
        if (res != null) {
            return Redirect("/Index");
        }

        return Page();
    }

    private async Task<IActionResult> HandleSaveExisting(int? versionId) {
        if (RecipeMetadata == null) {
            return Page();
        }
        var createRequest = new CreateRecipeRequest {
            Recipe = RecipeMetadata.ToRecipeDb(),
            Steps = RecipeSteps.Select(s => s.ToStepDb()).ToList(),
            Ingredients = RecipeIngredients.Select(i => i.ToIngredientDb()).ToList()
        };

        VersionId? redirectId;
        if (versionId == null) {
            redirectId = (await RecipeService.CreateRecipe(createRequest))?.VersionId;
        }
        else {
            redirectId =
                await RecipeService.UpdateRecipeVersion(new VersionId(versionId.Value), createRequest);
        }

        if (redirectId != null) {
            return Redirect($"/Recipe/{redirectId}");
        }

        return Page();
    }
}