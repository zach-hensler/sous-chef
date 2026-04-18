using System.Data.Common;
using System.Net;
using core;
using core.Data;
using core.Models;
using core.Models.DbModels;

namespace services;

public static class Utils {
    private static readonly Logging Log = new ();
    private static readonly ConnectionFactory ConnectionFactory = new();
    public static async Task<Response> SafeRun(string sourceWorkflow, Func<Task<Response>> handle) {
        try {
            return await handle();
        }
        catch (Exception ex) {
            await WriteError(sourceWorkflow, ex.Message);
            return new Response(HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    public static async Task<Response> SafeRun(string sourceWorkflow, Func<DbConnection, Task<Response>> handle) {
        try {
            await using var conn = ConnectionFactory.GetConnection();
            await conn.OpenAsync();
            var res = await handle(conn);
            await conn.CloseAsync();
            return res;
        }
        catch (Exception ex) {
            await WriteError(sourceWorkflow, ex.Message);
            return new Response(HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    public static async Task<Response<T>> SafeRun<T>(string sourceWorkflow, Func<DbConnection, Task<Response<T>>> handle) {
        try {
            await using var conn = ConnectionFactory.GetConnection();
            await conn.OpenAsync();
            var res = await handle(conn);
            await conn.CloseAsync();
            return res;
        }
        catch (Exception ex) {
            await WriteError(sourceWorkflow, ex.Message);
            return new Response<T>(HttpStatusCode.InternalServerError, ex.Message);
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
        }
        catch (Exception errorHistoryEx) {
            Log.LogError("Unable to write error message" + errorHistoryEx.Message);
        }
    }
}