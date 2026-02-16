using FluentMigrator;

namespace migrations;

[Migration(4, "Add recipe categories (and others)")]
public class RecipeCategories: Migration {
    public override void Up() {
        Execute.Sql(
            "ALTER TABLE recipes ADD COLUMN category TEXT NOT NULL DEFAULT 'Uncategorized';");
        Execute.Sql(
            "ALTER TABLE recipes ALTER COLUMN description DROP NOT NULL;");
    }

    public override void Down() {
        throw new NotImplementedException();
    }
}