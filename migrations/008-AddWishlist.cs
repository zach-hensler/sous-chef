using FluentMigrator;

namespace migrations;

[Migration(8, "Add Wishlist Table")]
public class AddWishlist: Migration {
    public override void Up() {
        Execute.Sql(
            """
            CREATE TABLE wishlist (
                wishlist_id SERIAL PRIMARY KEY,
                name TEXT NOT NULL UNIQUE,
                priority INT NOT NULL DEFAULT 0,
                reference TEXT NULL,
                completed BOOL NOT NULL DEFAULT false,
                created_at TIMESTAMP NOT NULL
            );
            """);
    }

    public override void Down() {
        throw new NotImplementedException();
    }
}