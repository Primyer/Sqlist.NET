#region License
// Copyright (c) 2021, Saleh Kawaf Kulla
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using FastMember;

using Sqlist.NET.Common;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;

namespace Sqlist.NET.Utilities
{
    /// <summary>
    ///     Represents the orientation of data mapping.
    /// </summary>
    public enum MappingOrientation
    {
        /// <summary>
        ///     Object oriented mapping. Means that the mapping will be proceeded according
        ///     to the properties in a given object as base. So, the queries will be forced
        ///     to return the a result that matches all the properties within the object.
        /// </summary>
        /// <remarks>
        ///     An exception is to be thrown if the query doesn't match the object.
        /// </remarks>
        ObjectOriented = 0,

        /// <summary>
        ///     Query oriented mapping. Means that the mapping will be proceeded according
        ///     to the fields return by a query. So, only the fields returned by the query
        ///     are to be mapped even if a given object has more propeties.
        ///     <para>
        ///         Note that this approach is more expensive.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     An exception is to be thrown if the object doesn't including properties that
        ///     match the query result.
        /// </remarks>
        QueryOriented = 1
    }

    internal static class DataParser
    {
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

        public static async Task<IEnumerable<T>> Object<T>(LazyDbDataReader lazyReader, MappingOrientation orientation, Action<T> altr)
        {
            var acsr = TypeAccessor.Create(typeof(T));
            var data = new List<T>();

            var rdr = await lazyReader.GetReaderAsync();
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
                    var name = names[i];
                    if (name is null) continue;

                    var val = reader.GetValue(i);
                    if (val is DBNull)
                        acsr[model, name] = null;
                    else
                        acsr[model, name] = val;
                }

                altr?.Invoke(model);
                data.Add(model);
            });
            return data;
        }

        private static string[] GetObjectOrientedNames<T>(IDataReader reader)
        {
            var props = typeof(T).GetProperties();
            var names = new string[reader.FieldCount];

            var count = 0;
            foreach (var prop in props)
            {
                if (prop.GetCustomAttribute<NotMappedAttribute>() != null)
                    continue;

                var attr = prop.GetCustomAttribute<ColumnAttribute>();
                names[reader.GetOrdinal(attr?.Name ?? prop.Name)] = prop.Name;

                count++;
            }

            if (count != names.Length)
                throw new InvalidOperationException("The result fields don't match the object properties.");

            return names;
        }

        private static string[] GetQueryOrientedNames<T>(IDataReader reader)
        {
            var props = typeof(T).GetProperties();
            var fields = new string[props.Length];
            var propNames = new string[props.Length];

            for (var i = 0; i < fields.Length; i++)
            {
                var prop = props[i];
                if (prop.GetCustomAttribute<NotMappedAttribute>() != null)
                    continue;

                var attr = prop.GetCustomAttribute<ColumnAttribute>();
                fields[i] = attr?.Name ?? prop.Name;
                propNames[i] = prop.Name;
            }

            var count = 0;
            var names = new string[reader.FieldCount];
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                var indx = Array.IndexOf(fields, name);

                if (indx != -1)
                {
                    names[i] = propNames[indx];
                    count++;
                }
            }

            if (count != names.Length)
                throw new InvalidOperationException("The object properties don't match the result fields.");

            return names;
        }
    }
}
