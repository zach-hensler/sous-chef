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
        foreach (var kvp in Request.Query) {
            if (!Enum.TryParse<AdminActions>(kvp.Key, out var action)) {
                Console.WriteLine($"Unexpected action: {kvp.Key} = {kvp.Value}");
                return Page();
            }
            return action switch {
                AdminActions.Migrate => await HandleMigrate(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        return Page();
    }

    public async Task<IActionResult> HandleMigrate() {
        var res = await _migrationService.Migrate();
        if ((int)res.StatusCode >= 300) {
            Console.WriteLine(res.ErrorMessage);
        }
        return Page();
    }
}