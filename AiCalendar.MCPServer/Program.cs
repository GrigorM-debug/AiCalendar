var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

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
