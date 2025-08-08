using AiCalendar.WebApi.Data;
using AiCalendar.WebApi.Data.Repository;
using AiCalendar.WebApi.Services.EventParticipants;
using AiCalendar.WebApi.Services.EventParticipants.Interfaces;
using AiCalendar.WebApi.Services.Events;
using AiCalendar.WebApi.Services.Events.Interfaces;
using AiCalendar.WebApi.Services.FindingAvailableSlots;
using AiCalendar.WebApi.Services.Users;
using AiCalendar.WebApi.Services.Users.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace AiCalendar.WebApi.Extensions
{
    public static class ServiceCollectionExtensions
    {
        //Extension method for adding the services
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ITokenProvider, TokenProvider>();
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IEventParticipantsService, EventParticipantsService>();
            services.AddScoped<IFindingAvailableSlotsService, FindingAvailableSlotsService>();
            // Add other necessary services
            return services;
        }

        //Extenstion method for adding the repositories
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            // Register repositories here
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            return services;
        }

        //Extension method for adding the database
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure Entity Framework Core with SQL Server
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });
            return services;
        }

        //Extension method for adding Swagger documentation with Swagger JWT authentication
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "AiCalendar API",
                    Version = "v1",
                    Description = "API for AiCalendar application"
                });

                var securitySchema = new OpenApiSecurityScheme()
                {
                    Name = "JWT Authentication",
                    Description = "Enter JWT Bearer token in this field",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    BearerFormat = "JWT",
                };

                options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securitySchema);

                var securityRequirement = new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = JwtBearerDefaults.AuthenticationScheme
                            }
                        },
                        []
                    }
                };

                options.AddSecurityRequirement(securityRequirement);
            });
            return services;
        }

        //Extension method for adding Rate Limiting
        public static IServiceCollection AddRateLimiting(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User?.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 10,
                            QueueLimit = 5,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
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
            return services;
        }
    }
}
