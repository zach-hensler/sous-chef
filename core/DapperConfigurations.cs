namespace core;

public static class DapperConfigurations {
    public static void Register() {
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
    }
}