using System.Data;
using System.Text.Json;
using core.Models.DbModels;
using Dapper;

namespace core;

public static class DapperConfigurations {
    public static void Register() {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        
        SqlMapper.AddTypeHandler(typeof(List<string>), new JsonStringListHandler());

        SqlMapper.AddTypeHandler(new IdMapper<RecipeId>());
        SqlMapper.AddTypeHandler(new IdMapper<VersionId>());
        SqlMapper.AddTypeHandler(new IdMapper<WishlistId>());
    }

    private class JsonStringListHandler : SqlMapper.ITypeHandler {
        public void SetValue(IDbDataParameter parameter, object value) {
            parameter.Value = JsonSerializer.Serialize(value);
        }

        public object Parse(Type destinationType, object value) {
            return JsonSerializer.Deserialize<List<string>>((value as string)!)!;
        }
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