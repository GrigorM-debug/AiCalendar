using System.Text;
using AiCalendar.Blazor.Components;
using AiCalendar.Blazor.Components.Utils;
using AiCalendar.Blazor.Services;
using AiCalendar.Blazor.Services.UserSession;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Tokens;

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

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("ApiClient"));

// Add services to the container.
builder.Services.AddRazorComponents(options =>
    {
        options.DetailedErrors = builder.Environment.IsDevelopment();
    })
    .AddInteractiveServerComponents();

builder.Services.AddBlazoredLocalStorage();

//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//    .AddCookie(options =>
//    {
//        options.LoginPath = "/users/login";
//        options.LogoutPath = "/users/logout";
//        options.ExpireTimeSpan = TimeSpan.FromDays(7); 
//        options.SlidingExpiration = true;
//    });

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]))
    };

    // The key part: read the token from the cookie
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Request.Cookies.TryGetValue("jwt", out var token);
            context.Token = token;
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddHttpContextAccessor();

builder.Services.AddDistributedMemoryCache();

//builder.Services.AddScoped<IUserSessionManager, UserSessionManager>();

builder.Services.AddServerSideBlazor();

builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

builder.Services.AddScoped<StatusCodeErrorHandeller>();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseStatusCodePagesWithReExecute("/status-code/{0}");

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
