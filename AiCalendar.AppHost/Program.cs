using Aspire.Hosting.ApplicationModel;
using Projects;
using Microsoft.Extensions.Hosting;
using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var sqlServer = builder
    .AddSqlServer("sql")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("ai-calendar-db");

var db = sqlServer.AddDatabase("DefaultConnection", "AiCalendarDb");

var web_api = builder.AddProject<AiCalendar_WebApi>("aicalendar-webapi")
    .WithReference(db)
    .WaitFor(db);

builder.AddProject<Projects.AiCalendar_MCPServer>("aicalendar-mcpserver")
    .WithReference(web_api)
    .WaitFor(web_api);

builder.Build().Run();
