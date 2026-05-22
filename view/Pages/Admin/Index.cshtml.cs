using core.Models;
using core.Models.DbModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using services;

namespace view.Pages.Admin;

public class AdminModel : PageModel {
    public ListErrorsResponse? ErrorRes { get; set; }
    public VersionInfoDb? LatestVersion { get; set; }

    private async Task LoadPageData() {
        ErrorRes = await AdminService.ListErrors(DateTime.UtcNow.AddMonths(-2));
        LatestVersion = await AdminService.GetLatestVersion();
    }
    public async Task<IActionResult> OnGetAsync() {
        await LoadPageData();
        return Page();
    }

    public async Task<IActionResult> OnPostMigrate() {
        await MigrationService.Migrate();
        await LoadPageData();
        return Partial("_AdminOnMigrateClick", this);
    }
}