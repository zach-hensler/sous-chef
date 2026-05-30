using System.Reflection;
using FluentMigrator;

namespace migrations;

[Migration(10, "Drop Enum Constraints")]
public class DropEnumConstraints : Migration {
    public override void Up() {
        Execute.Sql("ALTER TABLE recipes DROP CONSTRAINT IF EXISTS check_categories");
        Execute.Sql("ALTER TABLE recipes DROP CONSTRAINT IF EXISTS check_effort_levels");
        Execute.Sql("CREATE INDEX IF NOT EXISTS recipe_categories ON recipes USING HASH (category)");
        Execute.Sql("CREATE INDEX IF NOT EXISTS recipe_effort_levels ON recipes USING HASH (effort_level)");
    }

    public override void Down() {
        throw new NotImplementedException();
    }
}