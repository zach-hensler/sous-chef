# Sous Chef
A kitchen helper with recipe versioning
---
# Backlog

## Missing v1 Features - Core Features
- Compare Page
  - Add select input to change version
- backups
  - chron script to regularly backup to a usb drive
  - if more than x backups found, delete the oldest

## v2 Features - Bells and Whistles
- navbar updates
  - media query, on mobile collapse to hamburger button + "Sous Chef"
    - hamburger button reveals the rest of the options

Main Feed Updates
- Filtering by Category
- Searching
  - By name, description, and ingredients
- Show avg rating (for latest version?) on home page

- Export Recipe to PDF

Status Updates
- Add to graph
  - recipe versions added per month
  - new recipes added per month
- Add a "The Classics" section
  - top 3(?) recipes that have the most comments, but haven't been made in the last 2-3(?) months


## Unscheduled Ideas
- Linked Recipes
  - Link types (similar, pairs with, inspired by)
  - fk to two different recipes
  - recipe details will list linked recipes
- Additional Filters:
  - Star Rating, Effort Level
- Grid option for recipe list page
- Recipe drafts
- Uploading image for a recipe
  - Should be automatically scaled down before saving image
- Grocery List
  - adding recipe to grocery list
- Support marking a version as the "active version"
  - A recipe can have only one version be active
    - boolean on the recipe_versions row
  - On compare screen add a "make active" checkbox
  - List recipes should return the "active" version instead of the latest
  - On compare screen, you can change which one is active

## Tech Debt
- Better response model (current isn't wrong, just odd)
- Reconsider how versions are stored
  - instead of storing entire steps/ingredients, only store modified
  - each version would reference it's child ingredients/steps
  - would save on db space with a complexity tradeoff
- Build pipeline fixes
  - Include commit messages of commits within the release
  - Select major/minor and generate next version number instead of making user enter number manually
    - Or use commit prefixes (would make it easier to remember what all was changed)
  - Output version number to file during build, have UI display value in Footer


## Tools to research
- nginx
- upgraded server
- htmx


## Going Public - TBD if I'll actually do this
- Any user can read
- Only I can write
- Requires improved hosting requirements and bot-blocking
- Auth Ideas:
  - TS based authentication
    - have public version w/ readonly db user and "read-only" env var for view purposes
    - have tailnet exclusive version w/ full db user and no "read-only" env var
  - Password based authentication
    - hidden login form that requires specific password to enable "admin mode"
    - login form hidden for UX not for security
    - have to be more careful since both go through same API user

---
# TODO Add a setup guide here
