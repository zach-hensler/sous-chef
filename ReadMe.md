# Sous Chef
A kitchen helper with recipe versioning

---
# Features
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
# Entities
- recipes
  - Top level entity
  - name, description, time, effort level
- recipe_versions
  - 1:M recipe:recipe_versions
  - version_number, rating, created_at
- recipe_comments
    - 1:M recipe_version:comments
    - notes about that version (goods and bads)
- recipe_steps
  - 1:M recipe_version:steps
  - steps to follow for a specific recipe version
  - name, step_number, instruction
- recipe_ingredients
  - 1:M recipe_version:ingredients
  - ingredients for a specific recipe version
  - name, note, quantity, unit