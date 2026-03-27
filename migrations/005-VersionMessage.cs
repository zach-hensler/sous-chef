using FluentMigrator;

namespace migrations;

[Migration(5, "")]
public class VersionMessage: Migration {
    public override void Up() {
        Execute.Sql("ALTER TABLE recipe_versions ADD COLUMN message TEXT NOT NULL DEFAULT '';");
        Execute.Sql("ALTER TABLE recipe_versions DROP COLUMN version_number;");
    }

    public override void Down() {
        throw new NotImplementedException();
    }
}