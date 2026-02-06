using core;
using services;

namespace Helpers;

public static class Setup {
    public static async Task<IConnectionFactory> GetConnectionFactory() {
        Environment.SetEnvironmentVariable(
            EnvironmentVariables.ConnectionString,
            "User ID=test-user;Password=test-pass;Host=localhost;Port=5433;Database=tests;");
        await new MigrationService().Migrate();
        return new ConnectionFactory();
    }
}