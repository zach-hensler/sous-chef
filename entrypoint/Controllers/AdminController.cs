using services;

namespace program;

public class AdminController {
    private readonly MigrationService _migrationService = new();

    public void Register(WebApplication app) {
        app.MapPut("/admin/migrate", Migrate);
    }

    private async Task Migrate(HttpContext context) {
        Console.WriteLine("Starting Migration");
        var res = await _migrationService.Migrate();
        Console.WriteLine("Migration Complete");
        await res.WriteResponse(context.Response);
    }
}