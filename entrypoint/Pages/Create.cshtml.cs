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
public enum PostActions {
    Submit,
    NewIngredient,
    NewStep
}

public class CreateModel : PageModel {
    private RecipeService _recipeService = new(new ConnectionFactory());

    public record Recipe {
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required EffortLevels EffortLevel { get; init; }
    }
    
    [BindProperty]
    public Recipe RecipeMetadata { get; set; }

    [BindProperty]
    public List<ViewIngedient> RecipeIngredients { get; init; } = [];

    [BindProperty] public List<ViewStep> RecipeSteps { get; init; } = [];

    public void OnGet() {
    }

    public async Task<IActionResult> OnPostAsync() {
        Request.Query.TryGetValue("action", out var action);
        Enum.TryParse<PostActions>(action.ToString(), out var postAction);

        // TODO support removing steps from the list
        return postAction switch {
            PostActions.Submit => await HandleSubmit(),
            PostActions.NewIngredient => HandleNewIngredient(),
            PostActions.NewStep => HandleNewStep(),
            _ => throw new ArgumentOutOfRangeException()
        };
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

    private async Task<IActionResult> HandleSubmit() {
        var response = await _recipeService.CreateRecipe(new CreateRecipeRequest {
            Recipe = new CreateRecipeDb {
                Name = RecipeMetadata.Name,
                Description = RecipeMetadata.Description,
                TimeMinutes = 0,
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
        });

        if (response.StatusCode == HttpStatusCode.OK) {
            return RedirectToPage("./Create");
        }

        Console.WriteLine(response.ErrorMessage);
        return Page();
    }
}