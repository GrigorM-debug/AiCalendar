using System.Reflection;
using AiCalendar.WebApi.Data;
using AiCalendar.WebApi.Data.Repository;
using AiCalendar.WebApi.Services.Users;
using AiCalendar.WebApi.Services.Users.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

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
    }
}
