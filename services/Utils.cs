using System.Data.Common;
using System.Net;
using core;
using core.Data;
using core.Models.DbModels;

namespace services;

public static class Utils {
    private static readonly Logging Log = new ();
    private static readonly ConnectionFactory ConnectionFactory = new();
    public static async Task SafeRun(string sourceWorkflow, Func<Task> handle) {
        try {
            await handle();
        }
        catch (Exception ex) {
            await WriteError(sourceWorkflow, ex.Message);
        }
    }

    public static async Task SafeRun(string sourceWorkflow, Func<DbConnection, Task> handle) {
        try {
            await using var conn = ConnectionFactory.GetConnection();
            await conn.OpenAsync();
            await handle(conn);
            await conn.CloseAsync();
        }
        catch (Exception ex) {
            await WriteError(sourceWorkflow, ex.Message);
        }
    }

    public static async Task<T?> SafeRun<T>(string sourceWorkflow, Func<Task<T>> handle) {
        try {
            return await handle();
        }
        catch (Exception ex) {
            await WriteError(sourceWorkflow, ex.Message);
            return default;
        }
    }

    public static async Task<T?> SafeRun<T>(string sourceWorkflow, Func<DbConnection, Task<T>> handle) {
        try {
            await using var conn = ConnectionFactory.GetConnection();
            await conn.OpenAsync();
            var res = await handle(conn);
            await conn.CloseAsync();
            return res;
        }
        catch (Exception ex) {
            await WriteError(sourceWorkflow, ex.Message);
            return default;
        }
    }

    private static async Task WriteError(string sourceWorkflow, string message) {
        try {
            var conn = new ConnectionFactory().GetConnection();
            await conn.OpenAsync();
            await Common.ErrorHistory.Add(
                new ErrorHistoryDb {
                    Source = sourceWorkflow,
                    Message = message,
                    OccurredAt = DateTime.UtcNow
                },
                conn);
            Log.LogError($"Workflow '{sourceWorkflow}' encountered error '{message}'");
        }
        catch (Exception errorHistoryEx) {
            Log.LogError("Unable to write error message: " + errorHistoryEx.Message);
        }
    }
}