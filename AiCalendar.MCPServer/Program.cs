using AiCalendar.MCPServer;
using System.Buffers.Text;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddHttpClient("ApiClient", client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"];

    if (string.IsNullOrEmpty(baseUrl))
    {
        throw new InvalidOperationException("ApiSettings:BaseUrl is not configured in appsettings.json.");
    }
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddScoped<EventService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowApiClient", policy =>
    {
        policy
            .AllowAnyOrigin() //.WithOrigins("") // Specify the allowed origin if needed
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseCors("AllowApiClient");

app.Run();
