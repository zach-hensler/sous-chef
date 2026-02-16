using core;
using core.Models;

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
        public static CreateRecipeRequest CreateRecipeRequest() {
            List<CreateRecipeStepDb> steps = [];
            for (var i = 0; i < Primitive.Int(1, 4); i++) {
                steps.Add(new CreateRecipeStepDb {
                    Name = Primitive.String(),
                    Instruction = Primitive.String()
                });
            }

            List<CreateRecipeIngredientDb> ingredients = [];
            for (var i = 0; i < Primitive.Int(1, 4); i++) {
                ingredients.Add(new CreateRecipeIngredientDb {
                    Name = Primitive.String(),
                    Note = Primitive.String(),
                    Quantity = Primitive.Int(),
                    Unit = Primitive.String()
                });
            }
            
            return new CreateRecipeRequest {
                Recipe = new CreateRecipeDb {
                    Name = Primitive.String(),
                    Description = Primitive.String(),
                    TimeMinutes = Primitive.Int(),
                    EffortLevel = EffortLevels.Easy,
                    Category = Categories.Uncategorized
                },
                Steps = steps,
                Ingredients = ingredients
            };
        }

        public static class Db {
            public static ErrorHistoryDb ErrorHistoryDb() {
                return new ErrorHistoryDb {
                    source = Primitive.String(),
                    message = Primitive.String(),
                    occurred_at = Primitive.Date()
                };
            }
        }
    }
}