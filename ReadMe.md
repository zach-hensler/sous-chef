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
- Wishlist
  - List of items that I want to learn to make
  - Name, CreatedAt, Priority (int or enum), References (json list)
  - New page with list, create modals, delete modals, view details modal (details could be new page)
- "Stats" page
  - history of last x items that I made (based off of comments left)
  - Stats for:
    - you made x items this week/month/year
    - your top 3 most made items are ...
- navbar updates
  - media query, on mobile collapse to hamburger button + "Sous Chef"
    - hamburger button reveals the rest of the options
- Response Updates
  - Delete Response type
  - Update Utils.SafeRun to return T? instead

Main Feed Updates
- Filtering by Category
- Searching
  - By name, description, and ingredients
- Show avg rating (for latest version?) on home page

- Export Recipe to PDF


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
- ~~Added Compare Page with diff checking for ingredients and steps~~
- ~~Removed Semantic Recipe Versioning~~

---
# TODO Add a setup guide here
