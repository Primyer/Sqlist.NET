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
