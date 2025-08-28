using System.Runtime.CompilerServices;
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

var frontend = builder.AddProject<Projects.AiCalendar_Blazor>("aicalendar-blazor")
    .WaitFor(web_api)
    .WaitFor(mcp_server);

var node_exporter = builder
    .AddContainer("nodeexporter", "prom/node-exporter")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithHttpEndpoint(port: 9100, targetPort: 9100)
    .WaitFor(web_api)
    .WaitFor(mcp_server)
    .WaitFor(frontend);

var prometheus = builder
    .AddContainer("prometheus", "prom/prometheus")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithBindMount(source: Path.GetFullPath("prometheus.yml"), target: "/etc/prometheus/prometheus.yml")
    .WithBindMount(source: Path.GetFullPath("rules_files"), target: "/etc/prometheus/rules_files")
    .WithArgs("--config.file=/etc/prometheus/prometheus.yml")
    .WithHttpEndpoint(port: 9090, targetPort: 9090)
    .WaitFor(web_api)
    .WaitFor(mcp_server)
    .WaitFor(frontend)
    .WaitFor(node_exporter);

var alertmanager = builder
    .AddContainer("alertmanager", "prom/alertmanager")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithBindMount(source: Path.GetFullPath("alertmanager.yml"), target: "/config/alertmanager.yml")
    .WithVolume("alertmanager-data", "/data")
    .WithHttpEndpoint(port: 9093, targetPort: 9093)
    .WithArgs("--config.file=/config/alertmanager.yml")
    .WaitFor(prometheus);

var grafana = builder
    .AddContainer("grafana", "grafana/grafana")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithVolume("grafana-data", "/var/lib/grafana")
    .WithHttpEndpoint(port: 80, targetPort: 3000)
    .WithEnvironment("GF_SECURITY_ADMIN_USER", "admin")
    .WithEnvironment("GF_SECURITY_ADMIN_PASSWORD", "admin")
    .WaitFor(prometheus)
    .WaitFor(alertmanager);


builder.Build().Run();
