using core;
using core.Data;
using core.Models;
using core.Models.DbModels;
using core.Models.ServiceModels;

namespace Helpers;

public static class Extensions {
    public static string Random(this string[] l) {
        return l[Rand.Primitive.Int(0, l.Length)];
    }
}

public static class Rand {
    private static Random _random = new();

    public static class Primitive {
        public static int Int() {
            return _random.Next();
        }

        public static int Int(int min, int max) {
            return _random.Next(min, max);
        }

        public static bool Bool(int percentageSuccess = 50) {
            return Int(0, 100) <= 50;
        }

        public static char Char() {
            return (char)('a' + Int(0, 25));
        }

        public static string String(int? length = null) {
            var realLength = length ?? Int(1, 20);
            var s = "";
            for (var i = 0; i <= realLength; i++) {
                s += Char();
            }

            return s;
        }

        public static string String(int min, int max) {
            return String(Int(min, max));
        }

        public static DateTime Date() {
            return DateTime.UtcNow
                .AddMonths(-1*Int(0, 12))
                .AddDays(-1*Int(0, 12))
                .AddMinutes(-1*Int(0, 12))
                .AddSeconds(-1*Int(0, 12))
                .AddMilliseconds(-1*Int(0, 12));
        }
    }

    public static class Copy {
        public static readonly string[] DishAdjectives = ["Easy", "Satisfying", "Crunchy", "Fluffy"];
        public static readonly string[] DishBase = ["Omelette", "Enchilada", "Pasta", "Taco"];

        public static readonly string[] Verbs = ["Chop", "Prep", "Slice", "Marinate", "Cook"];

        public static readonly string[] IngredientAdjectives = ["Ground", "Whole", "Sliced", "Shredded", "Grated", "Red", "Green", "Yellow", "Fresh", "Frozen", "Pickled", "Ripe", "Raw"];
        public static readonly string[] MainIngredients = ["Chicken", "Beef", "Pork", "Egg", "Bacon", "Cheese", "Mushroom", "Broccoli"];
        public static readonly string[] SecondaryIngredients = ["Butter", "Olive Oil", "Garlic", "Onion", "Salt", "Tomato Paste", "Beans"];
        public static readonly string[] Units = ["cups", "tbsps", "tsps", "ounces", "pounds"];
        public static string RecipeName() {
            var name = "";
            if (Primitive.Bool(20)) {
                name += DishAdjectives.Random();
                name += " ";
            }
            if (Primitive.Bool()) {
                name += MainIngredients.Random();
                name += " ";
            }
            if (Primitive.Bool(20)) {
                name += SecondaryIngredients.Random();
                name += " ";
            }

            name += DishBase.Random();
            return name;
        }

        public static string IngredientName() {
            var name = Primitive.Bool() ? MainIngredients.Random() : SecondaryIngredients.Random();
            return $"{IngredientAdjectives.Random()} {IngredientAdjectives.Random()} {name}";
        }

        public static string StepName() {
            return $"{Verbs.Random()} {MainIngredients.Random()}";
        }

        public static string Sentence() {
            var sentence = "";
            for (var i = 0; i < Primitive.Int(4, 10); i++) {
                sentence += $"{Primitive.String(1, 6)} ";
            }

            return sentence.Trim() + ".";
        }

        public static string Paragraph() {
            var paragraph = "";
            for (var i = 0; i < Primitive.Int(1, 5); i++) {
                paragraph += $"{Sentence()} ";
            }

            return paragraph.Trim();
        }
    }

    public static class Domain {
        public static class Db {
            public static ErrorHistoryDb ErrorHistoryDb() {
                return new ErrorHistoryDb {
                    Source = Primitive.String(),
                    Message = Primitive.String(),
                    OccurredAt = Primitive.Date()
                };
            }

            public static RecipeDb RecipeDb() {
                return new RecipeDb {
                    RecipeId = new RecipeId(0),
                    Name = Copy.RecipeName(),
                    Description = Copy.Paragraph(),
                    TotalTimeMinutes = Primitive.Int(0, 120),
                    EffortLevel = (EffortLevels)Primitive.Int(0, 2),
                    Category = (Categories)Primitive.Int(0, 4),
                    OriginalAuthor = Primitive.String(),
                    ActiveTimeMinutes = 0
                };
            }

            public static RecipeCommentDb RecipeCommentDb(VersionId version) {
                return new RecipeCommentDb {
                    CommentId = 0,
                    VersionId = version,
                    Rating = Primitive.Int(0, 5),
                    Comment = Copy.Sentence(),
                    CreatedAt = Primitive.Date()
                };
            }

            public static RecipeVersionDb RecipeVersionDb(RecipeId recipeId) {
                return new RecipeVersionDb {
                    VersionId = new VersionId(0),
                    Message = Copy.Sentence(),
                    RecipeId = recipeId,
                    CreatedAt = Primitive.Date()
                };
            }

            public static StepDb RecipeStepDb(int idx) {
                return new StepDb {
                    RecipeId = new RecipeId(0),
                    VersionId = new VersionId(0),
                    Name = Copy.StepName(),
                    StepNumber = idx.ToString(),
                    Instruction = Copy.Paragraph()
                };
            }

            public static RecipeIngredientDb RecipeIngredientDb() {
                return new RecipeIngredientDb {
                    RecipeId = new RecipeId(0),
                    VersionId = new VersionId(0),
                    Name = Copy.IngredientName(),
                    Quantity = Primitive.Int(1, 20),
                    Unit = Copy.Units.Random()
                };
            }

            public static WishlistDb WishlistDb() {
                return new WishlistDb {
                    Name = Copy.RecipeName(),
                    Priority = 1,
                    Reference = Primitive.String(),
                    Completed = false,
                    WishlistId = new WishlistId(0),
                    CreatedAt = Primitive.Date()
                };
            }
        }
        
        public static class Requests {
            public static CreateRecipeRequest CreateRecipeRequest() {
                List<CreateStepDb> steps = [];
                for (var i = 0; i < Primitive.Int(1, 4); i++) {
                    steps.Add(Db.RecipeStepDb(i+1).ToCreateStepDb());
                }

                List<CreateIngredientDb> ingredients = [];
                for (var i = 0; i < Primitive.Int(1, 4); i++) {
                    ingredients.Add(Db.RecipeIngredientDb().ToCreateIngredientDb());
                }
            
                return new CreateRecipeRequest {
                    Recipe = Db.RecipeDb().ToCreateRecipeDb(),
                    Steps = steps,
                    Ingredients = ingredients
                };
            }

            public static CreateRecipeVersionRequest CreateRecipeVersionRequest(VersionId previousVersion) {
                return new CreateRecipeVersionRequest {
                    PreviousVersionId = previousVersion,
                    Recipe = Db.RecipeDb().ToCreateRecipeDb(),
                    Steps = [
                        Db.RecipeStepDb(1).ToCreateStepDb(),
                        Db.RecipeStepDb(2).ToCreateStepDb(),
                    ],
                    Ingredients = [
                        Db.RecipeIngredientDb().ToCreateIngredientDb(),
                        Db.RecipeIngredientDb().ToCreateIngredientDb()
                    ],
                    Message = Primitive.String()
                };
            }
        }
    }
}