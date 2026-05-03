using System.Data.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;

using services;
using core;
using Dapper;
using Xunit;

namespace Helpers;

public static class Setup {
    public static async Task<DbConnection> ResetAndGetDatabase() {
        await ResetAndSetupDatabase();
        var conn = new ConnectionFactory(true).GetConnection();
        await conn.OpenAsync(TestContext.Current.CancellationToken);

        return conn;
    }
    public static async Task ResetAndSetupDatabase() {
        Environment.SetEnvironmentVariable(EnvVars.ConnectionString, "User ID=test-user;Password=test-pass;Host=localhost;Port=5433;Database=tests;");

        var connFactory = new ConnectionFactory(true);
        await using var conn = connFactory.GetConnection();
        await conn.OpenAsync(TestContext.Current.CancellationToken);
        await conn.ExecuteAsync(
            """
            DROP SCHEMA public CASCADE;
            CREATE SCHEMA public;
            """);
        await conn.CloseAsync();
        DapperConfigurations.Register();

        if (await MigrationService.Migrate(true) != true) {
            throw new Exception("Unable to migrate");
        }
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