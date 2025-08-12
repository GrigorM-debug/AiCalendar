using AiCalendar.MCPServer;
using Microsoft.Extensions.Logging;
using System.Buffers.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using ModelContextProtocol.AspNetCore;

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

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 10,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.OnRejected = async (context, cancellationToken) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = retryAfter.TotalSeconds.ToString();

            ProblemDetailsFactory problemDetailsFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();

            Microsoft.AspNetCore.Mvc.ProblemDetails problemDetails =
                problemDetailsFactory.CreateProblemDetails(
                    context.HttpContext,
                    StatusCodes.Status429TooManyRequests,
                    "Too many requests",
                    detail: $"Too many requests. Please retry after ${retryAfter.TotalSeconds} seconds"
                );

            await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        }
        //// Custom rejection handling logic
        //context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        //context.HttpContext.Response.Headers["Retry-After"] = "60";

        //await context.HttpContext.Response.WriteAsync("Rate limit exceeded. Please try again later.", cancellationToken);
    };
});

builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<EventParticipantsService>();

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

app.MapMcp("/mcp");

app.UseCors("AllowApiClient");

app.Run();
