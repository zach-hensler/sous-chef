using core.Data;
using core.Models;

namespace services;

public static class RecipeVersionService {
    public static async Task<Response<RecipeVersionDb>> GetVersion(int versionId) {
        return await Utils.SafeRun(nameof(GetVersion), async (conn) => {
            var details = await Common.RecipeVersion.Get(versionId, conn);
            return new Response<RecipeVersionDb>(details);
        });
    }
}