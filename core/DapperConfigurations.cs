using System.Data;
using core.Models.DbModels;
using Dapper;

namespace core;

public static class DapperConfigurations {
    public static void Register() {
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        SqlMapper.AddTypeHandler(new IdMapper<RecipeId>());
        SqlMapper.AddTypeHandler(new IdMapper<VersionId>());
    }
    
    private class IdMapper<T> : SqlMapper.TypeHandler<T> where T:IdBase {
        public override void SetValue(IDbDataParameter parameter, T? id) {
            ArgumentNullException.ThrowIfNull(id);
            parameter.DbType = DbType.Int32;
            parameter.Value = id.Value;
        }

        public override T Parse(object dbValue) {
            return (T)Activator.CreateInstance(typeof(T), (int)dbValue)!;
        }
    }
}