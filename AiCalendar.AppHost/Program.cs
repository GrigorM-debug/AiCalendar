using Aspire.Hosting.ApplicationModel;
using Projects;
using Microsoft.Extensions.Hosting;
using Aspire.Hosting;
using Aspire.Hosting.Docker;

var builder = DistributedApplication.CreateBuilder(args);

var sqlServer = builder
    .AddSqlServer("sql")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("ai-calendar-db");

var db = sqlServer.AddDatabase("DefaultConnection", "AiCalendarDb");

var web_api = builder.AddProject<AiCalendar_WebApi>("aicalendar-webapi")
    .WithReference(db)
    .WaitFor(db);

var mcp_server = builder.AddProject<Projects.AiCalendar_MCPServer>("aicalendar-mcpserver")
    .WithReference(web_api)
    .WaitFor(web_api);

var prometheus = builder
    .AddContainer("prometheus", "prom/prometheus")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithBindMount(source: Path.GetFullPath("prometheus.yml"), target: "/etc/prometheus/prometheus.yml")
    .WithArgs("--config.file=/etc/prometheus/prometheus.yml")
    .WithHttpEndpoint(port: 9090, targetPort: 9090)
    .WaitFor(web_api)
    .WaitFor(mcp_server);

var grafana = builder
    .AddContainer("grafana", "grafana/grafana")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithVolume("grafana_data", "/var/lib/grafana")
    .WithHttpEndpoint(port: 3001, targetPort: 3000)
    .WithEnvironment("GF_SECURITY_ADMIN_USER", "admin")
    .WithEnvironment("GF_SECURITY_ADMIN_PASSWORD", "admin")
    .WaitFor(prometheus);

builder.Build().Run();
