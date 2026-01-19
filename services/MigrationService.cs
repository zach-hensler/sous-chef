using System.Net;
using core;
using core.Models;
using FluentMigrator.Runner;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using migrations;

namespace services;

public class MigrationService {
    private readonly string? _connectionString = Environment.GetEnvironmentVariable(EnvironmentVariables.ConnectionString);

    public async Task<Response> Migrate() {
        return await Utils.SafeRun(() => {
            if (string.IsNullOrWhiteSpace(_connectionString)) {
                return Task.FromResult(new Response(
                    HttpStatusCode.InternalServerError,
                    "Connection String not configured"));
            }

            var sp = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddPostgres()
                    .WithGlobalConnectionString(_connectionString)
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