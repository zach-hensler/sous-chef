using core.Data;

namespace services;

public static class StatService {
    public static async Task<List<TotalMonthlyCount>?> GetTotalCountsByMonth() {
        return await Utils.SafeRun(nameof(GetTotalCountsByMonth), async conn =>
            await GetStats.GetTotalCountsByMonth(conn));
    }
    public static async Task<List<RecipeMonthlyCount>?> GetRecentRecipeCounts(int months) {
        return await Utils.SafeRun(nameof(GetRecentRecipeCounts), async conn =>
            await GetStats.GetRecentRecipeCounts(months, conn));
    }
}