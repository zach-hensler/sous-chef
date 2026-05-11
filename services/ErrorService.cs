using core.Data;
using core.Models;

namespace services;

public static class ErrorService {
    public static async Task<ListErrorsResponse?> ListErrors(DateTime cutoff) {
        return await Utils.SafeRun(nameof(ListErrors), async conn => {
            var total = await Common.ErrorHistory.Count(cutoff, conn);
            var errors = await Common.ErrorHistory.List(0, 10, conn);
            return new ListErrorsResponse {
                Errors = errors,
                Total = total
            };
        });
    }
}