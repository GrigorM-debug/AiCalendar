using Aspire.Hosting.ApplicationModel;
using Projects;
using Microsoft.Extensions.Hosting;
using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

//var sqlServer = builder.AddSqlServer("sqlserver")
//    .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
//    .WithEnvironment("ACCEPT_EULA", "Y")
//    .WithEnvironment("SA_PASSWORD", "YourPassword123!")
//    .WithPassword("YourPassword123!") // for Aspire to inject into connection strings
//    .WithPersistentStorage()
//    .WithVolume("sql-volume");

var sqlServer = builder
    .AddSqlServer("sql")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("ai-calendar-db");

var db = sqlServer.AddDatabase("DefaultConnection", "AiCalendarDb");

builder.AddProject<AiCalendar_WebApi>("aicalendar-webapi")
    .WithReference(db)
    .WaitFor(db);

builder.Build().Run();
