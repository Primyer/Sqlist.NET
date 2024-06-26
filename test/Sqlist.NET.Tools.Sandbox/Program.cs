using Sqlist.NET.Extensions;
using Sqlist.NET.Tools.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
       .AddSqlist()
       .ForPostgreSQL(options =>
       {
           var connectionString = builder.Configuration.GetConnectionString("Default");
           options.SetConnectionString(connectionString ?? "");
       });

builder.UseSqlistTools()
       .Build()
       .Run();