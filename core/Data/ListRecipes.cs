using System.Data.Common;
using core.Models;
using Dapper;

namespace core.Data;

public static class ListRecipes {
    public static async Task<List<ListRecipesResponse.RecipeItem>> Get(int limit, int offset, DbConnection conn) {
        return (await conn.QueryAsync<ListRecipesResponse.RecipeItem>(@"
WITH latest_versions AS (
    SELECT
        recipe_id,
        version_id,
        created_at,
        ROW_NUMBER() OVER (PARTITION BY recipe_id ORDER BY created_at DESC) as row
    FROM recipe_versions
)
SELECT
    r.*,
    lv.version_id as LatestVersionId
FROM recipes r
INNER JOIN latest_versions lv ON lv.recipe_id = r.recipe_id
WHERE lv.row = 1
ORDER BY lv.created_at DESC
LIMIT @limit
OFFSET @offset;
                ",
                new { limit, offset }))
            .ToList();
    }
}