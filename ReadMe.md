# Sous Chef
A kitchen helper with recipe versioning
---
# Missing Features
## Current Priorities
- version comment support
  - leave a star rating w/ optional comment for a version
  - should show avg ratings on main feed
  - show all comments on recipe details page
  - ability to add comments from "recipe details"
- pipeline to create executable for local deployment
  - gh action to generate a build?
  - expose to local network from raspberry pi via tailscale
  - need to actually start using this to identify pain points
- database backups

## Secondary Priorities
- Recipe drafts
  - it's scary spending 10 minutes typing it all out and worrying that an error might happen
  - de-prioritized because editing helps fill this gap
- Create new recipe versions
- Rename EffortLevels from (Low, Medium, High) -> (Easy, Medium, Hard)
- Better Error Handling
- Admin Panel

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

# Tech Debt
- Better logging
- Better response model (current isn't wrong, just odd)

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
