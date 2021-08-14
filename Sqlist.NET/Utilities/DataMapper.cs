using FastMember;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Reflection;

namespace Sqlist.NET.Utilities
{
    internal static class DataMapper<T> where T : class, new()
    {
        public static IEnumerable<T> Parse(IDataReader reader)
        {
            var acsr = TypeAccessor.Create(typeof(T));
            var data = new List<T>();
            var names = GetNames(reader);

            while (reader.Read())
            {
                var model = new T();

                for (var i = 0; i < reader.FieldCount; i++)
                    acsr[model, names[i]] = reader.GetValue(i);

                data.Add(model);
            }
            return data;
        }

        private static string[] GetNames(IDataReader reader)
        {
            var props = typeof(T).GetProperties();
            var names = new string[props.Length];

            foreach (var prop in props)
            {
                var attr = prop.GetCustomAttribute<ColumnAttribute>();
                names[reader.GetOrdinal(attr?.Name ?? prop.Name)] = prop.Name;
            }
            return names;
        }
    }
}
