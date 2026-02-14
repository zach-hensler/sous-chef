# Sous Chef
A kitchen helper with recipe versioning
---
# Missing Features
## ISSUES
- can't update tailscale on laptop
- can't reach pi on tailnet
  - tailscale serve might work ok, and container just isn't running
- image built for amd64 instead of arm
  - sous-chef-app The requested image's platform (linux/amd64) does not match the detected host platform (linux/arm64/v8) and no specific platform was requested
- app doesn't seem to be starting (probably related to above)

## Current Priorities
- docker compose for running db and application on pi
- volume storage for db
- pi deployment w/ tailscale
- database backups

## Secondary Priorities
- Show avg rating (for latest version?) on home page
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
- ~~Adding Comments with ratings~~
- ~~Viewing Comments on Recipes~~
- ~~Creating Docker Image on GH Action~~

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