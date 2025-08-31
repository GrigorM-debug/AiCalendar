using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AiCalendar.Blazor.Services.UserSession
{
    public class UserSessionManager : IUserSessionManager
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserSessionManager(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task SignInUserAsync(string jwtToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                throw new InvalidOperationException("HttpContext is not available.");

            // Store JWT in HttpOnly cookie
            httpContext.Response.Cookies.Append("jwt", jwtToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Set to true in production
                SameSite = SameSiteMode.Strict,
                Expires = new JwtSecurityTokenHandler().ReadJwtToken(jwtToken).ValidTo.ToUniversalTime()
            });

            // Extract claims from JWT
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwtToken);
            var claims = token.Claims.ToList();

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = token.ValidTo.ToUniversalTime()
                });
        }

        public void SignOutUser()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                httpContext.Response.Cookies.Delete("jwt");
                httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        }
    }
}
