using FluentMigrator;

namespace migrations;

[Migration(2, "Move ratings to Recipe Comments")]
public class Ratings: Migration {
    public override void Up() {
        Execute.Sql("ALTER TABLE recipe_versions DROP COLUMN rating");
        Execute.Sql("ALTER TABLE recipe_comments ADD COLUMN rating INT NOT NULL DEFAULT 0");
        Execute.Sql("ALTER TABLE recipe_comments ALTER COLUMN comment DROP NOT NULL");
    }

    public override void Down() {
        throw new NotImplementedException();
    }
}