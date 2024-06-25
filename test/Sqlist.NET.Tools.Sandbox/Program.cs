using Sqlist.NET.Extensions;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;

builder.Services
    .AddSqlist()
    .ForPostgreSQL(o => o.SetConnectionString(config.GetConnectionString("Default") ?? ""))
    .AddSqlistTools();

var app = builder.Build();
app.Run();