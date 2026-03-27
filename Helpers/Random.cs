using core;
using core.Models;
using core.Models.DbModels;

namespace Helpers;

public static class Rand {
    private static Random _random = new();
    public static class Primitive {
        public static int Int() {
            return _random.Next();
        }

        public static int Int(int min, int max) {
            return _random.Next(min, max);
        }

        public static char Char() {
            return (char)('a' + Int(0, 25));
        }

        public static string String(int? length = null) {
            var realLength = length ?? Int(0, 20);
            var s = "";
            for (var i = 0; i <= realLength; i++) {
                s += Char();
            }

            return s;
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

    public static class Domain {
        public static class Db {
            public static RecipeDb RecipeDb() {
                return new RecipeDb {
                    RecipeId = new RecipeId(0),
                    Name = Primitive.String(),
                    Description = Primitive.String(),
                    TimeMinutes = Primitive.Int(0,120),
                    EffortLevel = (EffortLevels)Primitive.Int(0,2),
                    Category = (Categories)Primitive.Int(0,4)
                };
            }

            public static RecipeStepDb RecipeStepDb(int idx) {
                return new RecipeStepDb {
                    RecipeId = new RecipeId(0),
                    VersionId = new VersionId(0),
                    Name = Primitive.String(),
                    StepNumber = idx.ToString(),
                    Instruction = Primitive.String()
                };
            }

            public static RecipeIngredientDb RecipeIngredientDb() {
                return new RecipeIngredientDb {
                    RecipeId = new RecipeId(0),
                    VersionId = new VersionId(0),
                    Name = Primitive.String(),
                    Note = null,
                    Quantity = Primitive.Int(),
                    Unit = Primitive.String()
                };
            }
            public static ErrorHistoryDb ErrorHistoryDb() {
                return new ErrorHistoryDb {
                    Source = Primitive.String(),
                    Message = Primitive.String(),
                    OccurredAt = Primitive.Date()
                };
            }

            public static class Create {
                public static CreateRecipeStepDb CreateRecipeStepDb() {
                    return new CreateRecipeStepDb {
                        Name = Primitive.String(),
                        Instruction = Primitive.String()
                    };
                }

                public static CreateRecipeIngredientDb CreateRecipeIngredientDb() {
                    return new CreateRecipeIngredientDb {
                        Name = Primitive.String(),
                        Note = Primitive.String(),
                        Quantity = Primitive.Int(),
                        Unit = Primitive.String()
                    };
                }

                public static CreateRecipeDb CreateRecipeDb() {
                    return new CreateRecipeDb {
                        Name = Primitive.String(),
                        Description = Primitive.String(),
                        TimeMinutes = Primitive.Int(),
                        EffortLevel = EffortLevels.Easy,
                        Category = Categories.Uncategorized
                    };
                }
            }
        }
        public static class Requests {
            public static CreateRecipeRequest CreateRecipeRequest() {
                List<CreateRecipeStepDb> steps = [];
                for (var i = 0; i < Primitive.Int(1, 4); i++) {
                    steps.Add(Db.Create.CreateRecipeStepDb());
                }

                List<CreateRecipeIngredientDb> ingredients = [];
                for (var i = 0; i < Primitive.Int(1, 4); i++) {
                    ingredients.Add(Db.Create.CreateRecipeIngredientDb());
                }
            
                return new CreateRecipeRequest {
                    Recipe = Db.Create.CreateRecipeDb(),
                    Steps = steps,
                    Ingredients = ingredients
                };
            }

            public static CreateRecipeVersionRequest CreateRecipeVersionRequest(VersionId previousVersion) {
                return new CreateRecipeVersionRequest {
                    PreviousVersionId = previousVersion,
                    VersionType = (VersionType)Primitive.Int(0,
                        1),
                    Recipe = Db.Create.CreateRecipeDb(),
                    Steps = [
                        Db.Create.CreateRecipeStepDb(),
                        Db.Create.CreateRecipeStepDb()
                    ],
                    Ingredients = [
                        Db.Create.CreateRecipeIngredientDb(),
                        Db.Create.CreateRecipeIngredientDb()
                    ],
                    Message = Primitive.String()
                };
            }
        }
    }
}