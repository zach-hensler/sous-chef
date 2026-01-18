using System.Data.Common;
using Npgsql;

namespace core;

public interface IConnectionFactory {
    public DbConnection GetConnection();
}

public class ConnectionFactory : IConnectionFactory {
    private readonly string? _connectionString = Environment.GetEnvironmentVariable(EnvironmentVariables.ConnectionString);

    public DbConnection GetConnection() {
        return string.IsNullOrWhiteSpace(_connectionString)
            ? throw new ArgumentNullException(nameof(_connectionString), "Connection string not found.")
            : new NpgsqlConnection(_connectionString);
    }
}