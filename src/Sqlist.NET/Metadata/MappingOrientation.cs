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

namespace Sqlist.NET.Metadata
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
        ///     An exception is to be thrown if the object's properties doesn't match the query result.
        /// </remarks>
        QueryOriented = 1
    }
}
