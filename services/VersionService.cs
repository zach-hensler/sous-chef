using System.Net;
using core.Data;
using core.Models;
using core.Models.DbModels;

namespace services;

public static class VersionService {
    public static async Task<Response<RecipeDetails>> GetRecipeByVersion(VersionId versionId) {
        return await Utils.SafeRun(nameof(GetRecipeByVersion), async (conn) => {
            var data = await GetRecipeDetails.GetByVersion(versionId, conn);

            return new Response<RecipeDetails>(data);
        });
    }
    public static async Task<Response<RecipeVersionDb>> GetVersion(VersionId versionId) {
        return await Utils.SafeRun(
            nameof(GetVersion),
            async (conn) => new Response<RecipeVersionDb>(await Common.RecipeVersion.Get(versionId, conn)));
    }

    public static async Task<Response<List<RecipeVersionDb>>> List(VersionId versionId) {
        return await Utils.SafeRun(nameof(GetVersion), async (conn) => {
            var recipe = await Common.RecipeVersion.Get(versionId, conn);
            return new Response<List<RecipeVersionDb>>(await Common.RecipeVersion.List(recipe.RecipeId, conn));
        });
    }

    public static async Task<Response> AddComment(CreateRecipeCommentDb request) {
        return await Utils.SafeRun(nameof(AddComment), async (conn) => {
            if (!await Common.RecipeVersion.Exists(request.VersionId, conn)) {
                return new Response(HttpStatusCode.NotFound, $"Version '{request.VersionId}' not found");
            }

            await Common.RecipeComments.Create(request, conn);

            return new Response();
        });
    }

    public static async Task<Response> DeleteRecipeVersion(VersionId versionId) {
        // TODO conditionally delete the whole recipe if this is the only version
        return await Utils.SafeRun(nameof(DeleteRecipeVersion), async (conn) => {
            await Common.RecipeVersion.DeleteCascade(versionId, conn);

            return new Response();
        });
    }

}