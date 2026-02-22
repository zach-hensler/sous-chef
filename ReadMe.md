# Sous Chef
A kitchen helper with recipe versioning
---
# Missing Features
## ISSUES
- tailscale upgrade failing

# Current Priorities
- Recipe Versions
  - Version Selector on Details Page
  - Version Comparison View
- Display comments from previous versions on details page
- Bug w/ "Move Up" and "Move Down" buttons
- Make Ids into types so we don't accidentally assign in the wrong spots anymore

## Backlog
### UI/UX
- UI review/updates
  - Home Feed
  - Modals
  - Edit Page
- Filters on Home Feed
  - By: Star Rating, Category, Effort Level
- Show avg rating (for latest version?) on home page
- grid view for main screen
- navbar updates
  - icons for admin/create
  - rename "recipes" as "Sous Chef"
  - media query, on mobile collapse to hamburger button + "Sous Chef"
    - hamburger button reveals the rest of the options
- On mobile, clicking "new ingredient/new step" should open up the on screen keyboard

### Features
- Recipe drafts
- Recipe Searching
  - By name, description, and ingredients
- Uploading image for a recipe
  - Should be automatically scaled down before saving image
- Build pipeline fixes
  - Include commit messages of commits within the release
  - Select major/minor and generate next version number instead of making user enter number manually
    - Or use commit prefixes (would make it easier to remember what all was changed)
- Grocery List
  - adding recipe to grocery list
- Original author citation for recipes
- Export Recipe to PDF
- Delete Dialog should have "Delete this Version" and "Delete Entire Recipe"
- Support marking a version as the "active version"
  - A recipe can have only one version be active
    - boolean on the recipe_versions row
  - On create screen add a "make active" checkbox
  - List recipes should return the "active" version instead of the latest
  - On compare screen, you can change which one is active

### Tech Debt
- Better logging
- Better response model (current isn't wrong, just odd)
- Reconsider how versions are stored
  - instead of storing entire steps/ingredients, only store modified
  - each version would reference it's child ingredients/steps
  - would save on db space with a complexity tradeoff

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

---
# Build pipeline
- push changes to a branch
  - aggregate changes on that branch
  - when ready for a new release to be created, merge branch to main
- on merge to main:
  - get new version number?
  - gh action builds docker image
  - gh action stores image as a build artifact
- raspberry pi pulls down new image, and runs new version