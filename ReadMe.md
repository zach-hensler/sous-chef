# Sous Chef
A kitchen helper with recipe versioning

---
# Features
1. Recipe logging
2. Recipe versioning
3. Search by ingredients
4. Search by recipes
5. Random recipe (meeting criteria)

## Recipe History
- can view the steps/ingredients for each version
- can view the notes/rating for each version
- can compare the differences between versions

---
# Tables
## recipes
- recipe_id pk
- name
- description
- image_key
- time
- effort_level
- created_at

## recipe_versions
- recipe_id fk
- version_id SERIAL pk
- version_number (major/minor upgrades?)
- unique (recipe_id, version_number)
- created_at

## recipe_comments
- recipe_id fk
- version_id fk
- comment_id pk serial int?
- comment
- created_at


## recipe_steps
- recipe_id fk
- version_number fk
- name text
- step_number int
- compound pk (recipe_id, version, name)
- unique (recipe_id, version, step_number)
- step text

## recipe_ingredients
`Note: this is duplicated on every new version.  Might be okay`
- recipe_id fk recipes
- version_number fk
- name
- compound pk (recipe_id, version, name)
- note
- quantity float
- unit text
