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
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Xunit;

namespace Helpers;

public static class Setup {
    public static async Task<DbConnection> ResetAndGetDatabase(bool migrate = true) {
        await ResetAndSetupDatabase(migrate);
        var conn = new ConnectionFactory(true).GetConnection();
        await conn.OpenAsync(TestContext.Current.CancellationToken);

        return conn;
    }
    public static async Task ResetAndSetupDatabase(bool migrate = true) {
        Environment.SetEnvironmentVariable(EnvVars.ConnectionString, "User ID=test-user;Password=test-pass;Host=localhost;Port=5433;Database=postgres;");

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

        if (migrate && await MigrationService.Migrate(true) != true) {
            throw new Exception("Unable to migrate");
        }
    }

    public static PageContext GetPageContext(string? key = null, string? value = null) {
        var stringValues = new Dictionary<string, StringValues>();
        if (key != null) {
            stringValues.Add(key, new StringValues(value));
        }
        var context = new PageContext(new ActionContext {
            ActionDescriptor = new ActionDescriptor(),
            RouteData = new RouteData(),
            ModelState = { },
            HttpContext = new DefaultHttpContext {
                Request = {
                    Query = new QueryCollection(stringValues)
                },
                Response = {
                    Headers = {}
                }
            }
        });
        context.ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());
        
        return context;
    }

    public static IModelMetadataProvider MetadataProvider() {
        return new EmptyModelMetadataProvider();
    }
}