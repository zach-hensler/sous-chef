using System.Net;
using core;
using core.Models;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using migrations;

namespace services;

public static class MigrationService {
    private static readonly string? ConnectionString = Environment.GetEnvironmentVariable(EnvVars.ConnectionString);

    public static async Task<Response> Migrate(bool testEnv = false) {
        return await Utils.SafeRun(nameof(Migrate), () => {
            if (string.IsNullOrWhiteSpace(ConnectionString)) {
                return Task.FromResult(new Response(
                    HttpStatusCode.InternalServerError,
                    "Connection String not configured"));
            }

            if (testEnv && !ConnectionString.Contains("test")) {
                throw new Exception("Configured to run as a test env, but a real connection string was provided.");
            }

            var sp = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddPostgres()
                    .WithGlobalConnectionString(ConnectionString)
                    .ScanIn(typeof(Initial).Assembly).For.Migrations())
                .BuildServiceProvider();

            using (var scope = sp.CreateScope()) {
                var runner =  scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
                runner.MigrateUp();
            }

            return Task.FromResult(new Response());
        });
    }
}