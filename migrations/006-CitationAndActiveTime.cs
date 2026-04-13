using FluentMigrator;

namespace migrations;

[Migration(6, "Add extra tracking fields")]
public class CitationAndActiveTime: Migration {
    public override void Up() {
        Execute.Sql(@"
ALTER TABLE recipes
ADD COLUMN active_time_minutes INT NOT NULL DEFAULT 0;

ALTER TABLE recipes
RENAME COLUMN time_minutes TO total_time_minutes;

UPDATE recipes
SET active_time_minutes = total_time_minutes
WHERE 1=1;

ALTER TABLE recipes
ADD COLUMN original_author TEXT NULL;");
    }

    public override void Down() {
        throw new NotImplementedException();
    }
}