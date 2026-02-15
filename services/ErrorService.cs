using core.Data;
using core.Models;

namespace services;

public static class ErrorService {
    public static async Task<Response<ListErrorsResponse>> ListErrors() {
        return await Utils.SafeRun(nameof(ListErrors), async (conn) => {
            var total = await Common.ErrorHistory.Count(conn);
            var errors = await Common.ErrorHistory.List(0, 10, conn);
            return new Response<ListErrorsResponse>(new ListErrorsResponse {
                Errors = errors,
                Total = total
            });
        });
    }
}