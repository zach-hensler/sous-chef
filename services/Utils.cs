using System.Net;
using core;
using core.Models;
using Microsoft.AspNetCore.Http;

namespace services;

public static class Utils {
    public static async Task<Response> SafeRun(Func<Task<Response>> handle) {
        try {
            return await handle();
        }
        catch (Exception ex) {
            return new Response(HttpStatusCode.InternalServerError, ex.Message);
        }
    }
    public static async Task<Response<T>> SafeRun<T>(Func<Task<Response<T>>> handle, bool recordError = true) {
        try {
            return await handle();
        }
        catch (Exception ex) {
            return new Response<T>(HttpStatusCode.InternalServerError, ex.Message);
        }
    }
}