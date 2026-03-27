# Sous Chef
A kitchen helper with recipe versioning
---
# Backlog

## Missing v1 Features - Core Features
- Original author citation for recipes
- Track "active time" vs "total time"
- Compare Page
  - Add select input to change version

## v2 Going Public
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


## v3 Features - Ease of Use
Main Feed Updates
- Filters on Home Feed
  - By: Star Rating, Category, Effort Level
  - Searching on Home Feed
    - By name, description, and ingredients
  - Show avg rating (for latest version?) on home page
- navbar updates
  - icons for admin/create
  - rename "recipes" as "Sous Chef"
  - media query, on mobile collapse to hamburger button + "Sous Chef"
    - hamburger button reveals the rest of the options
- Recipe List page should use grid instead of list


## Backlog
- Convert steps to a jsonb list
- Recipe drafts
- Uploading image for a recipe
  - Should be automatically scaled down before saving image
- Grocery List
  - adding recipe to grocery list
- Export Recipe to PDF
- Support marking a version as the "active version"
  - A recipe can have only one version be active
    - boolean on the recipe_versions row
  - On create screen add a "make active" checkbox
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

## Completed Features
- ~~Create New Recipes~~
- ~~List Recipes~~
- ~~Delete Recipes (with confirmation modal)~~
- ~~Basic Theming (css vars for colors and spacing)~~
- ~~Improved UX on Recipe creation~~
  - ~~Autofocus when adding new steps/ingredients~~
  - ~~Larger inputs & text areas~~
  - ~~Step deletion/re-ordering~~
  - ~~Ingredient re-ordering~~
- ~~Basic Admin Panel~~
- ~~Adding Comments with ratings~~
- ~~Viewing Comments on Recipes~~
- ~~Creating Docker Image on GH Action~~
- ~~Able to run image on PI and reach from external device~~
- ~~Rename EffortLevels from (Low, Medium, High) -> (Easy, Medium, Hard)~~
- ~~Error History log on Admin Page~~
- ~~Tmux on Server for session management~~
- ~~Recipe Versions support~~
- ~~Display comments from previous versions on details page~~
- ~~Bug w/ "Move Up" and "Move Down" buttons~~
- ~~Delete Dialog should have "Delete this Version" and "Delete Entire Recipe"~~
- ~~Autofocus rating dialog and remove default~~
- ~~Auto focus after adding/removing/ordering on Create page~~

---
# Initial Roadmap
## v1.0 Features
1. Recipe versioning
2. UI for recipe viewing
3. UI for recipe creation
4. UI for recipe editing
5. UI for version comparison
6. Recipe comments/ratings

## Follow-up features
1. Uploading images (with thumbnail creation)
2. Search by ingredients
3. Search by recipe name/description
4. Random recipe (meeting criteria)

### Recipe History
- can view the steps/ingredients for each version
- can view the notes/rating for each version
- can compare the differences between versions
