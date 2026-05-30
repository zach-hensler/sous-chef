using FluentMigrator;

namespace migrations;

public static class MigrationInfo {
    public static long GetLatestVersion() {
        long version = 1;
        foreach (var types in typeof(Initial).Assembly.GetTypes()) {
            var attributes = types.GetCustomAttributes(typeof(MigrationAttribute), true);
            if (attributes.Length == 0) {
                continue;
            }
            var attribute = (MigrationAttribute)attributes.First();
            
            if (attribute.Version > version) {
                version = attribute.Version;
            }
        }

        return version;
    }
}