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
}

public record ViewStep {
    public required string Name { get; init; }
    public required string Instruction { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CreateActions {
    Submit,
    NewIngredient,
    RemoveIngredient,
    NewStep,
    RemoveStep,
    MoveStepDown,
    MoveStepUp
}

public class CreateModel : PageModel {
    private RecipeService _recipeService = new(new ConnectionFactory());

    public record Recipe {
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required EffortLevels EffortLevel { get; init; }
        public required int Time { get; init; }
}
    
    [BindProperty]
    public Recipe RecipeMetadata { get; set; }

    [BindProperty]
    public List<ViewIngedient> RecipeIngredients { get; set; } = [];

    [BindProperty] public List<ViewStep> RecipeSteps { get; set; } = [];
    public int? VersionId { get; set; }

    public string GetBaseRoute() {
        var baseRoute = "/Create";
        if (VersionId != null) {
            baseRoute += $"/{VersionId}";
        }

        return baseRoute;
    }

    public async Task<IActionResult> OnGet(int? id) {
        VersionId = id;
        if (id == null) {
            return Page();
        }

        var res = await _recipeService.GetRecipe(id.Value);
        if ((int)res.StatusCode > 200 || res.Data == null) {
            Console.WriteLine(res.ErrorMessage);
        }
        else {
            RecipeMetadata = new Recipe {
                Name = res.Data!.Name,
                Description = res.Data.Description,
                EffortLevel = res.Data.EffortLevel,
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
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id) {
        VersionId = id;
        foreach (var kvp in Request.Query) {
            if (!Enum.TryParse<CreateActions>(kvp.Key, out var action)) {
                Console.WriteLine($"Unexpected action: {kvp.Key} = {kvp.Value}");
                return Page();
            }

            return action switch {
                CreateActions.Submit => await HandleSubmit(id),
                CreateActions.NewIngredient => HandleNewIngredient(),
                CreateActions.NewStep => HandleNewStep(),
                CreateActions.RemoveIngredient => RemoveIngredient(kvp.Value.ToString()),
                CreateActions.RemoveStep => RemoveStep(kvp.Value.ToString()),
                CreateActions.MoveStepDown => MoveStepDown(kvp.Value.ToString()),
                CreateActions.MoveStepUp => MoveStepUp(kvp.Value.ToString()),
                _ => throw new ArgumentOutOfRangeException()
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

    private async Task<IActionResult> HandleSubmit(int? versionId) {
        var createRequest = new CreateRecipeRequest {
            Recipe = new CreateRecipeDb {
                Name = RecipeMetadata.Name,
                Description = RecipeMetadata.Description,
                TimeMinutes = RecipeMetadata.Time,
                EffortLevel = RecipeMetadata.EffortLevel
            },
            Steps =
                RecipeSteps
                    .Select(s => new CreateRecipeStepDb {
                        Name = s.Name,
                        Instruction = s.Instruction
                    })
                    .ToList(),
            Ingredients =
                RecipeIngredients
                    .Select(i => new CreateRecipeIngredientDb {
                        Name = i.Name,
                        Note = "",
                        Quantity = i.Quantity,
                        Unit = i.Unit
                    })
                    .ToList()
        };

        Response? response;
        int redirectId;
        if (versionId == null) {
            var createRes = await _recipeService.CreateRecipe(createRequest);
            redirectId = createRes.Data;
            response = createRes;
        }
        else {
            response = await _recipeService.UpdateRecipe(versionId.Value, createRequest);
            redirectId = (int)versionId;
        }

        if (response.StatusCode == HttpStatusCode.OK) {
            return Redirect("/Recipe/" + redirectId);
        }

        Console.WriteLine(response.ErrorMessage);
        return Page();
    }
}