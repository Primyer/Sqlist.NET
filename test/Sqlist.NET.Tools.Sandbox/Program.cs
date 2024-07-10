using Npgsql.NameTranslation;

using Sqlist.NET.Extensions;
using Sqlist.NET.Migration.Extensions;
using Sqlist.NET.Migration.Tests.Metadata;
using Sqlist.NET.TestResources.Properties;
using Sqlist.NET.Tools.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
       .AddSqlist()
       .ForPostgreSQL(options =>
       {
           var connectionString = builder.Configuration.GetConnectionString("Default") ?? "";

           options.SetConnectionString(connectionString);
           options.ConfigureDataSource(builder =>
           {
               builder.MapEnum<UserStatus>("user_status", new NpgsqlNullNameTranslator());
           });
       })
       .WithMigration(options =>
       {
           var assembly = typeof(Consts).Assembly;

           options.SetMigrationAssembly(assembly, Consts.ScriptsRscPath + ".v3");
           options.SetDataMigrationRoadmapAssembly(assembly, Consts.RoadmapRscPath);
       });

builder.UseSqlistTools()
       .Build()
       .Run();