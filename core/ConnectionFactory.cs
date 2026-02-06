using System.Data.Common;
using Npgsql;

namespace core;

public interface IConnectionFactory {
    public DbConnection GetConnection();
}

public class ConnectionFactory(bool testEnv = false) : IConnectionFactory {
    private readonly string? _connectionString = Environment.GetEnvironmentVariable(EnvironmentVariables.ConnectionString);

    public DbConnection GetConnection() {
        if (string.IsNullOrWhiteSpace(_connectionString)) {
            throw new ArgumentNullException(nameof(_connectionString), "Connection string not found.");
        }
        if (testEnv && !_connectionString.Contains("test")) {
            throw new Exception("Configured to run as a test env, but a real connection string was provided.");
        }
        return new NpgsqlConnection(_connectionString);
    }
}