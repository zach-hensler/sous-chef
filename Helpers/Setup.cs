using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;

using services;
using core;
using Dapper;

namespace Helpers;

public static class Setup {
    public static async Task<ConnectionFactory> ResetAndGetDatabase() {
        Environment.SetEnvironmentVariable(EnvironmentVariables.ConnectionString, "User ID=test-user;Password=test-pass;Host=localhost;Port=5433;Database=tests;");

        var connFactory = new ConnectionFactory(true);
        await using var conn = connFactory.GetConnection();
        await conn.OpenAsync();
        await conn.ExecuteAsync(
            """
            DROP SCHEMA public CASCADE;
            CREATE SCHEMA public;
            """);
        await conn.CloseAsync();
        DapperConfigurations.Register();

        var migrationRes = await MigrationService.Migrate(true);
        return
            !string.IsNullOrWhiteSpace(migrationRes.ErrorMessage)
                ? throw new Exception(migrationRes.ErrorMessage)
                : connFactory;
    }

    public static PageContext GetPageContext(string key, string? value = null) {
        return new PageContext(new ActionContext {
            ActionDescriptor = new ActionDescriptor(),
            RouteData = new RouteData(),
            ModelState = { },
            HttpContext = new DefaultHttpContext {
                Request = {
                    Query = new QueryCollection(
                        new Dictionary<string, StringValues> { [key] = new(value) })
                }
            }
        });
    }
}