using core;
using Dapper;
using services;

namespace Helpers;

public static class Setup {
    public static async Task<IConnectionFactory> GetConnectionFactory() {
        Environment.SetEnvironmentVariable(
            EnvironmentVariables.ConnectionString,
            "User ID=test-user;Password=test-pass;Host=localhost;Port=5433;Database=tests;");
        var connFactory = new ConnectionFactory(true);
        await using var conn = connFactory.GetConnection();
        await conn.OpenAsync();
        await conn.ExecuteAsync(
            """
            DROP SCHEMA public CASCADE;
            CREATE SCHEMA public;
            """);
        await new MigrationService().Migrate(true);
        return connFactory;
    }
}