using FluentMigrator;

namespace migrations;

[Migration(1, "Initial Schema")]
public class Initial: Migration {
    public override void Up() {
        Execute.Sql(@"
CREATE TABLE recipes (
    recipe_id int PRIMARY KEY,
    name text NOT NULL,
    description text NOT NULL,
    image_key text NOT NULL,
    time_minutes int NOT NULL,
    effort_level text NOT NULL,
    created_at timestamp NOT NULL
);

CREATE TABLE recipe_versions (
    version_id SERIAL PRIMARY KEY,
    version_number text NOT NULL,
    recipe_id int NOT NULL,
    created_at timestamp NOT NULL,

    UNIQUE (recipe_id, version_number),
    CONSTRAINT fk_recipe FOREIGN KEY (recipe_id) REFERENCES recipes(recipe_id)
);

CREATE TABLE recipe_comments (
    comment_id serial PRIMARY KEY,
    recipe_id int NOT NULL,
    version_id int NOT NULL,
    comment text NOT NULL,
    created_at timestamp NOT NULL,

    CONSTRAINT fk_recipe FOREIGN KEY (recipe_id) REFERENCES recipes(recipe_id),
    CONSTRAINT fk_version FOREIGN KEY (version_id) REFERENCES recipe_versions(version_id)
);

CREATE TABLE recipes_steps (
    recipe_id int NOT NULL,
    version_id int NOT NULL,
    name text NOT NULL,
    step_number text NOT NULL,
    step text NOT NULL,    

    PRIMARY KEY (recipe_id, version_id, step_number),
    CONSTRAINT fk_recipe FOREIGN KEY (recipe_id) REFERENCES recipes(recipe_id),
    CONSTRAINT fk_version FOREIGN KEY (version_id) REFERENCES recipe_versions(version_id)
);

CREATE TABLE recipe_ingredients (
    recipe_id int NOT NULL,
    version_id int NOT NULL,
    name text NOT NULL,
    note text NOT NULL,
    quantity float NOT NULL,
    unit text NOT NULL,
    
    PRIMARY KEY (recipe_id, version_id, name),
    CONSTRAINT fk_recipe FOREIGN KEY (recipe_id) REFERENCES recipes(recipe_id),
    CONSTRAINT fk_version FOREIGN KEY (version_id) REFERENCES recipe_versions(version_id)
)

");
    }

    public override void Down() {
        throw new NotImplementedException();
    }
}