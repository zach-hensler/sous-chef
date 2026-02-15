using FluentMigrator;

namespace migrations;

[Migration(3, "Add Error Tracking")]
public class ErrorTracking: Migration {
    public override void Up() {
        Execute.Sql(
            """
            CREATE TABLE IF NOT EXISTS error_history (
                source TEXT NOT NULL,
                message TEXT NOT NULL,
                occurred_at TIMESTAMP NOT NULL
            );
            """);
        Execute.Sql(
            """
            UPDATE recipes
            SET effort_level = 'Easy'
            WHERE effort_level = '0' OR effort_level = 'Low'
            """);
        Execute.Sql(
            """
            UPDATE recipes
            SET effort_level = 'Medium'
            WHERE effort_level = '1'
            """);
        Execute.Sql(
            """
            UPDATE recipes
            SET effort_level = 'Hard'
            WHERE effort_level = '2'
            """);
    }

    public override void Down() {
        throw new NotImplementedException();
    }
}