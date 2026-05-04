using System.Data;
using System.Text.Json;
using core.Models.DbModels;
using Dapper;

namespace core;

public static class DapperConfigurations {
    public static void Register() {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        
        SqlMapper.AddTypeHandler(typeof(List<string>), new JsonStringListHandler());
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());

        SqlMapper.AddTypeHandler(new IdMapper<RecipeId>());
        SqlMapper.AddTypeHandler(new IdMapper<VersionId>());
        SqlMapper.AddTypeHandler(new IdMapper<WishlistId>());
    }
    
    public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly> {
        public override DateOnly Parse(object value) {
            switch (value) {
                case DateOnly dateOnly:
                    return dateOnly;
                case DateTime dateTime:
                    return DateOnly.FromDateTime(dateTime.Date);
                case string s:
                    return DateOnly.FromDateTime(DateTime.Parse(s).Date);
            }

            throw new Exception($"Unable to convert value '{value}'");
        }

        public override void SetValue(IDbDataParameter parameter, DateOnly value) {
            parameter.Value = value.ToDateTime(TimeOnly.MinValue);
            parameter.DbType = DbType.Date;
        }
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