using FluentMigrator;

namespace migrations;

[Migration(9, "Fix Migration Values")]
public class FixEnums: Migration {
    public override void Up() {
        Execute.Sql(
            """
            UPDATE recipes
            SET category =
                    CASE
                        WHEN category = '0' THEN 'Uncategorized'
                        WHEN category = '1' THEN 'Entree'
                        WHEN category = '2' THEN 'Side'
                        WHEN category = '3' THEN 'Drink'
                        WHEN category = '4' THEN 'Desert'
                        ELSE category
                    END;
            """);
        
        Execute.Sql(
            """
            ALTER TABLE recipes
            ADD CONSTRAINT check_categories
                CHECK (category IN ('Uncategorized', 'Entree', 'Side', 'Drink', 'Desert'));
            """);
    
        Execute.Sql(
            """
            UPDATE recipes
            SET effort_level =
                CASE
                    WHEN effort_level = '0' THEN 'Easy'
                    WHEN effort_level = '1' THEN 'Medium'
                    WHEN effort_level = '2' THEN 'Hard'
                    ELSE effort_level
                END;
            """);
        
        Execute.Sql(
            """
            ALTER TABLE recipes
            ADD CONSTRAINT check_effort_levels
                CHECK (effort_level IN ('Easy', 'Medium', 'Hard'));
            """);
    }

    public override void Down() {
        throw new NotImplementedException();
    }
}