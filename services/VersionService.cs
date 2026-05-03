using System.Net;
using core.Data;
using core.Models;
using core.Models.DbModels;

namespace services;

public static class VersionService {
    public static async Task<RecipeDetails?> GetRecipeByVersion(VersionId versionId) {
        return await Utils.SafeRun(nameof(GetRecipeByVersion),
            async conn => await GetRecipeDetails.GetByVersion(versionId, conn));
    }
    public static async Task<RecipeVersionDb?> GetVersion(VersionId versionId) {
        return await Utils.SafeRun(
            nameof(GetVersion),
            async conn => await Common.Version.Get(versionId, conn));
    }

    public static async Task<List<RecipeVersionDb>> List(VersionId versionId) {
        return
            await Utils.SafeRun(nameof(GetVersion), async conn => {
                var recipe = await Common.Version.Get(versionId, conn);
                return await Common.Version.List(recipe.RecipeId, conn);
            })
            ?? [];
    }

    public static async Task AddComment(CreateRecipeCommentDb request) {
        await Utils.SafeRun(nameof(AddComment), async conn => {
            if (!await Common.Version.Exists(request.VersionId, conn)) {
                throw new Exception($"Version '{request.VersionId}' not found");
            }

            await Common.RecipeComments.Create(request, conn);
        });
    }

    public static async Task DeleteRecipeVersion(VersionId versionId) {
        await Utils.SafeRun(nameof(DeleteRecipeVersion), async conn => {
            var versions = await Common.Version.ListOtherVersions(versionId, conn);
            if (versions.Count > 1) {
                await Common.Version.DeleteCascade(versionId, conn);    
            }
            else {
                await Common.Recipe.DeleteCascade(versions.First().RecipeId, conn);
            }
        });
    }

    public static async Task DeleteEntireRecipe(VersionId versionId) {
        await Utils.SafeRun(nameof(DeleteRecipeVersion), async conn => {
            await Common.Recipe.DeleteCascadeFromVersion(versionId, conn);
        });
    }
}