using System.Text.Json.Serialization;
using core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using services;

namespace sous_chef.Pages;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AdminActions {
    Migrate
}

public class AdminModel : PageModel {
    public ListErrorsResponse? ErrorRes { get; set; }

    private async Task LoadPageData() {
        ErrorRes = await ErrorService.ListErrors();
        
    }
    public async Task<IActionResult> OnGetAsync() {
        await LoadPageData();
        return Page();
    }
    
    public async Task<IActionResult> OnPostAsync() {
        foreach (var kvp in Request.Query) {
            if (!Enum.TryParse<AdminActions>(kvp.Key, out var action)) {
                await LoadPageData();
                return Page();
            }
            var actionResult = action switch {
                AdminActions.Migrate => await HandleMigrate(),
                _ => throw new ArgumentOutOfRangeException()
            };
            await LoadPageData();
            return actionResult;
        }

        await LoadPageData();
        return Redirect("Admin");
    }

    public async Task<IActionResult> HandleMigrate() {
        await MigrationService.Migrate();
        return Page();
    }
}