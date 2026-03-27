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
            async (conn) => new Response<RecipeVersionDb>(await Common.Version.Get(versionId, conn)));
    }

    public static async Task<Response<List<RecipeVersionDb>>> List(VersionId versionId) {
        return await Utils.SafeRun(nameof(GetVersion), async (conn) => {
            var recipe = await Common.Version.Get(versionId, conn);
            return new Response<List<RecipeVersionDb>>(await Common.Version.List(recipe.RecipeId, conn));
        });
    }

    public static async Task<Response> AddComment(CreateRecipeCommentDb request) {
        return await Utils.SafeRun(nameof(AddComment), async (conn) => {
            if (!await Common.Version.Exists(request.VersionId, conn)) {
                return new Response(HttpStatusCode.NotFound, $"Version '{request.VersionId}' not found");
            }

            await Common.RecipeComments.Create(request, conn);

            return new Response();
        });
    }

    public static async Task<Response> DeleteRecipeVersion(VersionId versionId) {
        return await Utils.SafeRun(nameof(DeleteRecipeVersion), async (conn) => {
            var versions = await Common.Version.ListOtherVersions(versionId, conn);
            if (versions.Count > 1) {
                await Common.Version.DeleteCascade(versionId, conn);    
            }
            else {
                await Common.Recipe.DeleteCascade(versions.First().RecipeId, conn);
            }

            return new Response();
        });
    }

    public static async Task<Response> DeleteEntireRecipe(VersionId versionId) {
        return await Utils.SafeRun(nameof(DeleteRecipeVersion), async (conn) => {
            await Common.Recipe.DeleteCascadeFromVersion(versionId, conn);

            return new Response();
        });
    }
}