using System.Data.Common;
using core.Models;
using Dapper;

namespace core.Data;

public static class Common {
    public static class Recipe {
        public static async Task<int> Create(CreateRecipeDb recipe, DbConnection conn) {
            return await conn.ExecuteScalarAsync<int>(
                """
                INSERT INTO recipes
                    (name, description, time_minutes, effort_level)
                VALUES
                    (@Name, @Description, @TimeMinutes, @EffortLevel)
                RETURNING recipe_id;
                """,
                new { recipe.Name, recipe.Description, recipe.TimeMinutes, EffortLevel = recipe.EffortLevel.ToString() });
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
                    (version_id, name, step_number, step)
                VALUES
                    (@versionId, @name, @stepIndex, @step)
                """,
                new {
                    versionId,
                    stepIndex,
                    step.Name,
                    step.Step
                });
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
}






