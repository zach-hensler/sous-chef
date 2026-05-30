using core.Models.DbModels;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace view.Pages.Create;

public class CreateIngredientListModel : PageModel {
    public required List<CreateIngredientDb> Ingredients { get; set; }
    public required string AutoFocusId { get; set; }
}