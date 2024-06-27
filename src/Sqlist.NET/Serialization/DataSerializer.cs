using FastMember;

using Sqlist.NET.Annotations;
using Sqlist.NET.Metadata;
using Sqlist.NET.Utilities;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace Sqlist.NET.Serialization
{

    internal static class DataSerializer
    {
        public static async Task<IEnumerable<T>> Json<T>(LazyDbDataReader lazyReader)
        {
            var data = new List<T>();

            await lazyReader.IterateAsync(reader =>
            {
                var value = JsonSerializer.Deserialize<T>(reader.GetString(0));
                data.Add(value!);
            });
            return data;
        }

        public static async Task<IEnumerable<T>> Primitive<T>(LazyDbDataReader lazyReader)
        {
            var type = typeof(T);
            var data = new List<T>();

            await lazyReader.IterateAsync(reader =>
            {
                var value = (T)Convert.ChangeType(reader.GetValue(0), type);
                data.Add(value);
            });
            return data;
        }

        public static async Task<IEnumerable<T>> Object<T>(LazyDbDataReader lazyReader, MappingOrientation orientation, Action<T>? altr)
        {
            var acsr = TypeAccessor.Create(typeof(T));
            var data = new List<T>();

            var rdr = await lazyReader.Reader;
            var names = orientation switch
            {
                MappingOrientation.ObjectOriented => GetObjectOrientedNames<T>(rdr),
                _ => GetQueryOrientedNames<T>(rdr)
            };

            await lazyReader.IterateAsync(reader =>
            {
                var model = Activator.CreateInstance<T>();

                for (var i = 0; i < reader.FieldCount; i++)
                {
                    var field = names[i];
                    if (field is null) continue;

                    var val = reader.GetValue(i);
                    if (val is DBNull)
                        acsr[model, field.Name] = null;
                    else
                        acsr[model, field.Name] = field.Parse(val);
                }

                altr?.Invoke(model);
                data.Add(model);
            });
            return data;
        }

        private static SerializationField[] GetObjectOrientedNames<T>(DbDataReader reader)
        {
            var fields = new SerializationField[reader.FieldCount];

            var count = 0;
            foreach (var prop in typeof(T).GetProperties())
            {
                if (prop.GetCustomAttribute<NotMappedAttribute>() is not null)
                    continue;

                var attr = prop.GetCustomAttribute<ColumnAttribute>();
                fields[reader.GetOrdinal(attr?.Name ?? prop.Name)] = Serialize(prop);

                count++;
            }

            if (count != fields.Length)
                throw new InvalidOperationException("The result fields don't match the object properties.");

            return fields;
        }

        private static SerializationField[] GetQueryOrientedNames<T>(DbDataReader reader)
        {
            var props = typeof(T).GetProperties();

            var serFields = new SerializationField[props.Length];
            var dbColumns = new string[props.Length];

            for (var i = 0; i < dbColumns.Length; i++)
            {
                var prop = props[i];
                if (prop.GetCustomAttribute<NotMappedAttribute>() is not null)
                    continue;

                var attr = prop.GetCustomAttribute<ColumnAttribute>();

                dbColumns[i] = attr?.Name ?? prop.Name;
                serFields[i] = Serialize(prop);
            }

            var count = 0;
            var fields = new SerializationField[reader.FieldCount];

            for (var i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                var indx = Array.IndexOf(dbColumns, name);

                if (indx != -1)
                {
                    fields[i] = serFields[indx];
                    count++;
                }
                else throw new InvalidOperationException($"The object properties don't match the result fields. '{name}' is missing in '{typeof(T).FullName}'.");
            }

            return fields;
        }

        private static SerializationField Serialize(PropertyInfo property)
        {
            var jsonAttr = property.GetCustomAttribute<JsonAttribute>();
            var enumAttr = property.GetCustomAttribute<EnumerationAttribute>();

            SerializationField field;

            if (jsonAttr is not null)
                field = new JsonField(property.PropertyType);

            else if (enumAttr is not null)
                field = new EnumField(property.PropertyType);
            else
                field = new SerializationField();

            field.Name = property.Name;

            return field;
        }
    }
}
