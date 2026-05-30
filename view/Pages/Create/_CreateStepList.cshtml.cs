using core.Models.DbModels;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace view.Pages.Create;

public class CreateStepListModel : PageModel {
    public required List<CreateStepDb> Steps { get; set; }
    public required string AutoFocusId { get; set; }
}