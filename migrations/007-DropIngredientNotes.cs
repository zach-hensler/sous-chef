using FluentMigrator;

namespace migrations;

[Migration(7, "Drop ingredient notes")]
public class DropIngredientNotes: Migration {
    public override void Up() {
        Execute.Sql("ALTER TABLE recipe_ingredients DROP COLUMN note;");
    }

    public override void Down() {
        throw new NotImplementedException();
    }
}