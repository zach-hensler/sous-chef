using System.Data.Common;
using core.Models.DbModels;
using Dapper;

namespace core.Data;

public record RecipeMonthlyCount {
    public required RecipeId RecipeId { get; init; }
    public required string Name { get; init; }
    public required int Count { get; init; }
    public required float AvgScore { get; init; }
}

public record TotalMonthlyCount {
    public required DateOnly MonthStart { get; init; }
    public required int Count { get; init; }
}

public static class GetStats {
    public static async Task<List<TotalMonthlyCount>> GetTotalCountsByMonth(DbConnection conn) {
        return (await conn.QueryAsync<TotalMonthlyCount>(
                """
                SELECT to_char(created_at, 'MM-01-YYYY') as month_start, count(*)
                FROM recipe_comments
                GROUP BY month_start
                ORDER BY month_start;
                """))
            .ToList();
    }
    public static async Task<List<RecipeMonthlyCount>> GetRecentRecipeCounts(int months, DbConnection conn) {
        return (await conn.QueryAsync<RecipeMonthlyCount>(
                $"""
                SELECT r.name, AVG(c.rating) as avg_score, count(*)
                FROM recipe_comments c
                INNER JOIN recipe_versions v on c.version_id = v.version_id
                INNER JOIN recipes r on r.recipe_id = v.recipe_id
                WHERE c.created_at > (now() - INTERVAL '{months} MONTH')
                GROUP BY r.name, r.recipe_id
                ORDER BY count(*) DESC;
                """))
            .ToList();
    }
}