using System.Data.Common;
using core.Models.ServiceModels;
using Dapper;

namespace core.Data;

public static class ListRecipesData {
    public static async Task<List<ListRecipesResponse.RecipeItem>> Get(ListRecipesRequest request, DbConnection conn) {
        var categoryFilter = request.CategoryFilter?.ToString();
        return (await conn.QueryAsync<ListRecipesResponse.RecipeItem>(
            """
                SELECT
                    r.*,
                    MAX(rv.version_id) as LatestVersionId,
                    AVG(c.rating) as AvgScore
                FROM recipes r
                INNER JOIN recipe_versions rv ON rv.recipe_id = r.recipe_id
                LEFT JOIN recipe_comments c on rv.version_id = c.version_id
                WHERE (@categoryFilter is null OR r.category = @CategoryFilter)
                GROUP BY r.recipe_id
                ORDER BY MAX(rv.created_at) DESC;
                """,
                new { categoryFilter }))
            .ToList();
    }
}