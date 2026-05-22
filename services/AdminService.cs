using core.Data;
using core.Models;
using core.Models.DbModels;

namespace services;

public static class AdminService {
    public static async Task<VersionInfoDb?> GetLatestVersion() {
        return await Utils.SafeRun(nameof(GetLatestVersion),
            async conn => await Common.VersionInfo.GetLatest(conn));
    }
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