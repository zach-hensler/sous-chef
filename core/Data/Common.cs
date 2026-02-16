using System.Data.Common;
using core.Models;
using Dapper;

namespace core.Data;

public static class Common {
    public static class Recipe {
        public static async Task<RecipeDb> Get(int recipeId, DbConnection conn) {
            return await conn.QuerySingleAsync<RecipeDb>(
                "SELECT * FROM recipes WHERE recipe_id = @recipeId LIMIT 1;",
                new { recipeId });
        }

        public static async Task<int> Create(CreateRecipeDb recipe, DbConnection conn) {
            return await conn.ExecuteScalarAsync<int>(
                """
                INSERT INTO recipes
                    (name, description, time_minutes, effort_level, category)
                VALUES
                    (@Name, @Description, @TimeMinutes, @EffortLevel, @Category)
                RETURNING recipe_id;
                """,
                new {
                    recipe.Name,
                    recipe.Description,
                    recipe.TimeMinutes,
                    EffortLevel = recipe.EffortLevel.ToString(),
                    Category = recipe.Category.ToString()
                });
        }

        public static async Task Update(int id, CreateRecipeDb recipe, DbConnection conn) {
            await conn.ExecuteAsync(
                """
                UPDATE recipes
                SET name = @Name,
                    description = @Description,
                    time_minutes = @TimeMinutes,
                    effort_level = @EffortLevel,
                    category = @Category
                WHERE recipe_id = @id;
                """,
                new { id, recipe.Name, recipe.Description, recipe.EffortLevel, recipe.TimeMinutes, recipe.Category });
        }

        public static async Task DeleteCascade(int recipeId, DbConnection conn) {
            await conn.ExecuteAsync(
                "DELETE FROM recipes WHERE recipe_id = @recipeId",
                new { recipeId });
        }

        public static async Task<List<string>> ListVersions(int recipeId, DbConnection conn) {
            return
                (await conn.QueryAsync<string>(
                    """
                    SELECT version_number
                    FROM recipe_versions
                    WHERE recipe_id = @recipeId
                    """,
                    new { recipeId }))
                .ToList();
        }
    }

    public static class RecipeVersion {
        public static async Task<bool> Exists(int versionId, DbConnection conn) {
            var count = await conn.QuerySingleAsync<int>(
                "SELECT count(*) FROM recipe_versions WHERE version_id = @versionId",
                new { versionId });
            return count > 0;
        }
        public static async Task<RecipeVersionDb> Get(int versionId, DbConnection conn) {
            return await conn.QuerySingleAsync<RecipeVersionDb>(
                "SELECT * FROM recipe_versions WHERE version_id = @versionId",
                new { versionId });
        }
        public static async Task<RecipeVersionDb> GetLatest(int recipeId, DbConnection conn) {
            return await conn.QuerySingleAsync<RecipeVersionDb>(
                """
                SELECT *
                FROM recipe_versions
                WHERE recipe_id = @recipeId
                ORDER BY created_at
                LIMIT 1;
                """,
                new { recipeId });
        }
        public static async Task<int> Create(CreateRecipeVersionDb version, DbConnection conn) {
            return await conn.ExecuteScalarAsync<int>(
                """
                INSERT INTO recipe_versions
                (version_number, recipe_id, created_at)
                VALUES
                    (@versionNumber, @recipeId, @createdAt)
                RETURNING version_id;
                """,
                new {
                    version.RecipeId,
                    version.VersionNumber,
                    version.CreatedAt
                });
        }
    }

    public static class RecipeSteps {
        public static async Task Create(CreateRecipeStepDb step, int stepIndex, int versionId, DbConnection conn) {
            await conn.ExecuteAsync(
                """
                INSERT INTO recipe_steps
                    (version_id, name, step_number, instruction)
                VALUES
                    (@versionId, @name, @stepIndex, @instruction)
                """,
                new {
                    versionId,
                    stepIndex,
                    step.Name,
                    step.Instruction
                });
        }

        public static async Task Delete(int versionId, DbConnection conn) {
            await conn.ExecuteAsync(
                "DELETE FROM recipe_steps WHERE version_id = @versionId;",
                new { versionId });
        }
        
        public static async Task<List<RecipeStepDb>> Get(int versionId, DbConnection conn) {
            return
                (await conn.QueryAsync<RecipeStepDb>(
                    """
                    SELECT *
                    FROM recipe_steps
                    WHERE version_id = @versionId
                    """,
                    new { versionId }))
                .ToList();
        }
    }

    public static class RecipeIngredients {
        public static async Task Create(CreateRecipeIngredientDb ingredient, int versionId, DbConnection conn) {
            await conn.ExecuteAsync(
                """
                INSERT INTO recipe_ingredients
                    (version_id, name, note, quantity, unit)
                VALUES
                    (@versionId, @name, @note, @quantity, @unit)
                """,
                new {
                    versionId,
                    ingredient.Name,
                    ingredient.Note,
                    ingredient.Quantity,
                    ingredient.Unit
                });
        }
        
        public static async Task Delete(int versionId, DbConnection conn) {
            await conn.ExecuteAsync(
                "DELETE FROM recipe_ingredients WHERE version_id = @versionId;",
                new { versionId });
        }
        
        public static async Task<List<RecipeIngredientDb>> Get(int versionId, DbConnection conn) {
            return
                (await conn.QueryAsync<RecipeIngredientDb>(
                    """
                    SELECT *
                    FROM recipe_ingredients
                    WHERE version_id = @versionId
                    """,
                    new { versionId }))
                .ToList();
        }
    }

    public static class RecipeComments {
        public static async Task<int> Create(CreateRecipeCommentDb comment, DbConnection conn) {
            return await conn.ExecuteScalarAsync<int>(
                """
                INSERT INTO recipe_comments
                    (version_id, rating, comment, created_at)
                VALUES
                    (@versionId, @rating, @comment, @createdAt)
                RETURNING comment_id
                """,
                new { comment.VersionId, comment.Rating, comment.Comment, comment.CreatedAt });
        }

        public static async Task<List<RecipeCommentDb>> Get(int versionId, DbConnection conn) {
            return (await conn.QueryAsync<RecipeCommentDb>(
                "SELECT * FROM recipe_comments WHERE version_id = @versionId",
                new { versionId }))
                .ToList();
        }
    }

    public static class ErrorHistory {
        public static async Task Add(ErrorHistoryDb errorHistory, DbConnection conn) {
            await conn.ExecuteAsync(
                "INSERT INTO error_history (source, message, occurred_at) VALUES (@source, @message, @occurred_at)",
                new { errorHistory.source, errorHistory.message, errorHistory.occurred_at });
        }

        public static async Task<int> Count(DbConnection conn) {
            return await conn.QuerySingleAsync<int>("SELECT count(*) FROM error_history");
        }

        public static async Task<List<ErrorHistoryDb>> List(int offset, int count, DbConnection conn) {
            return
                (await conn.QueryAsync<ErrorHistoryDb>(
                "SELECT * FROM error_history ORDER BY occurred_at DESC LIMIT @count OFFSET @offset",
                new { count, offset }))
                .ToList();
        }
    }
}
