using core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using services;

namespace view.Pages.Admin;

public class AdminModel : PageModel {
    public ListErrorsResponse? ErrorRes { get; set; }

    private async Task LoadPageData() {
        ErrorRes = await ErrorService.ListErrors(DateTime.UtcNow.AddMonths(-2));
    }
    public async Task<IActionResult> OnGetAsync() {
        await LoadPageData();
        return Page();
    }

    public async Task<IActionResult> OnPostMigrate() {
        await MigrationService.Migrate();
        await LoadPageData();
        return Partial("_AdminErrorFeed", this);
    }
}