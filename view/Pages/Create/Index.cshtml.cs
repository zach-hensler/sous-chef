using System.Text.Json.Serialization;
using core.Models;
using core.Models.DbModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using services;

namespace view.Pages.Create;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Direction {
    Up,
    Down
}

// TODO convert this into the create db models
public class CreateModel : PageModel {
    [BindProperty] public string AutoFocusId { get; set; } = "";
    [BindProperty] public string? UpdateMessage { get; set; } = "";
    [BindProperty] public CreateRecipeDb? RecipeMetadata { get; set; }

    [BindProperty] public List<CreateIngredientDb> Ingredients { get; set; } = [];

    [BindProperty] public List<CreateStepDb> Steps { get; set; } = [];
    public VersionId? VersionId { get; set; }

    public async Task<IActionResult> OnGet(int? versionId) {
        if (versionId == null) {
            return Page();
        }

        VersionId = new VersionId(versionId.Value);
        var res = await VersionService.GetRecipeByVersion(VersionId);
        if (res == null) {
            return Page();
        }

        RecipeMetadata = res.RecipeMetadata.ToCreateRecipeDb();
        Ingredients = res.Ingredients.Select(i => i.ToCreateIngredientDb()).ToList();
        Steps = res.Steps.Select(s => s.ToCreateStepDb()).ToList();

        return Page();
    }

    public IActionResult OnPostAddStep(List<CreateStepDb> steps) {
        Steps = steps;
        Steps.Add(new CreateStepDb {
            Name = "",
            Instruction = ""
        });
        AutoFocusId = $"step-name-{Steps.Count - 1}";
        return Partial(
            "_CreateStepList",
            new CreateStepListModel {
                Steps = Steps,
                AutoFocusId = AutoFocusId
            });
    }

    /// <summary>
    /// TODO try giving these fields "non-index based ids"
    /// might let the browser handle this itself
    /// </summary>
    private void SyncStepAttemptedValues() {
        // TextAreas don't automatically sync and must be manually synced
        for (var i = 0; i < Steps.Count; i++) {
            ModelState[$"{nameof(Steps)}[{i}].{nameof(CreateStepDb.Name)}"]?.AttemptedValue = Steps[i].Name;
            ModelState[$"{nameof(Steps)}[{i}].{nameof(CreateStepDb.Instruction)}"]?.AttemptedValue = Steps[i].Instruction;
        }
    }

    public IActionResult OnPostRemoveStep(List<CreateStepDb> steps, [FromQuery] int index) {
        Steps.RemoveAt(index);
        AutoFocusId = $"step-name-{Math.Clamp(index - 1, 0, Steps.Count)}";

        SyncStepAttemptedValues();
        return Partial(
            "_CreateStepList",
            new CreateStepListModel {
                Steps = Steps,
                AutoFocusId = AutoFocusId
            });
    }

    public IActionResult OnPostMoveStep(
        List<CreateStepDb> steps, [FromQuery] int index, [FromQuery] Direction direction) {
        switch (direction) {
            case Direction.Up:
                var step = Steps[index];
                Steps.RemoveAt(index);
                Steps.Insert(index - 1, step);
                AutoFocusId = $"step-name-{index - 1}";
                break;
            case Direction.Down:
                step = Steps[index];
                Steps.RemoveAt(index);
                Steps.Insert(index + 1, step);
                AutoFocusId = $"step-name-{index + 1}";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }
        SyncStepAttemptedValues();
        return Partial(
            "_CreateStepList",
            new CreateStepListModel {
                Steps = Steps,
                AutoFocusId = AutoFocusId
            });
    }

    public IActionResult OnPostRemoveIngredient(List<CreateIngredientDb> ingredients, int index) {
        Ingredients = ingredients;
        Ingredients.RemoveAt(index);
        AutoFocusId = $"ingredient-name-{Math.Clamp(index - 1, 0, Ingredients.Count)}";

        return Partial(
            "_CreateIngredientList",
            new CreateIngredientListModel {
                Ingredients = Ingredients,
                AutoFocusId = AutoFocusId
            });
    }

    public IActionResult OnPostAddIngredient(List<CreateIngredientDb> ingredients) {
        Ingredients = ingredients;
        Ingredients.Add(new CreateIngredientDb {
            Name = "",
            Quantity = 0f,
            Unit = ""
        });
        AutoFocusId = $"ingredient-name-{Ingredients.Count - 1}";
        return Partial(
            "_CreateIngredientList",
            new CreateIngredientListModel {
                Ingredients = Ingredients,
                AutoFocusId = AutoFocusId
            });
    }

    public async Task<IActionResult> OnPostSaveNew(
        int? versionId,
        CreateRecipeDb? recipeMetadata,
        List<CreateIngredientDb> ingredients,
        List<CreateStepDb> steps) {
        if (versionId == null || RecipeMetadata == null) {
            return Page();
        }

        VersionId = new VersionId(versionId.Value);
        var res = await RecipeService.CreateRecipeVersion(
            new CreateRecipeVersionRequest {
                PreviousVersionId = VersionId,
                Recipe = RecipeMetadata,
                Steps = Steps,
                Ingredients = Ingredients,
                Message = UpdateMessage ?? ""
            });
        if (res != null) {
            Response.Headers["HX-Redirect"] = $"/Recipe/{res}";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostSaveExisting(
        int? versionId,
        CreateRecipeDb? recipeMetadata,
        List<CreateIngredientDb> ingredients,
        List<CreateStepDb> steps) {
        if (RecipeMetadata == null) {
            return Content("");
        }
        var createRequest = new CreateRecipeRequest {
            Recipe = RecipeMetadata,
            Steps = Steps,
            Ingredients = Ingredients
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
            Response.Headers["HX-Redirect"] = $"/Recipe/{redirectId}";
        }

        return Page();
    }
}