using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using services;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AdminActions {
    Migrate
}

public class AdminModel : PageModel {
    private readonly MigrationService _migrationService = new MigrationService();

    public async Task<IActionResult> OnPostAsync() {
        Request.Query.TryGetValue("action", out var action);
        Enum.TryParse<AdminActions>(action.ToString(), out var postAction);
        return postAction switch {
            AdminActions.Migrate => await HandleMigrate(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public async Task<IActionResult> HandleMigrate() {
        var res = await _migrationService.Migrate();
        if ((int)res.StatusCode < 300) {
            Console.WriteLine(res.ErrorMessage);
        }
        return Page();
    }
}