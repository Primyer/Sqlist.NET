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

using Microsoft.Extensions.DependencyInjection;

using Sqlist.NET.Infrastructure;

using System;

namespace Sqlist.NET.Extensions
{
    /// <summary>
    ///     Provides the extension API to configure Sqlist services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds the Sqlist services to the context.
        /// </summary>
        /// <param name="services">The service collection to add to.</param>
        /// <param name="config">The configuration action to set up the Sqlist options.</param>
        public static void AddSqlist(this IServiceCollection services, Action<DbOptionsBuilder> config)
        {
            var builder = new DbOptionsBuilder();
            config.Invoke(builder);

            services.AddSingleton(builder.Options);
            services.AddScoped<DbCoreBase>();
        }
    }
}
