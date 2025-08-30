using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace AiCalendar.Blazor.Services.UserSession
{
    public class UserSessionManager : IUserSessionManager
    { 
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserSessionManager(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }


        public async Task SignInUserAsync(string jwtToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                // Handle the case where HttpContext is not available
                throw new InvalidOperationException("HttpContext is not available.");
            }

            httpContext.Response.Cookies.Append("jwt", jwtToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Use this in production with HTTPS
                SameSite = SameSiteMode.Strict,
                Expires = new JwtSecurityTokenHandler().ReadJwtToken(jwtToken).ValidTo.ToUniversalTime()
            });

            // 2. Sign the user into ASP.NET's authentication system.
            var claims = GetClaimsFromJwt(jwtToken);
            var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            await httpContext.SignInAsync(claimsPrincipal);
        }

        public void SignOutUser()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext != null)
            {
                httpContext.Response.Cookies.Delete("jwt");
                httpContext.SignOutAsync();
            }
        }

        private IEnumerable<Claim> GetClaimsFromJwt(string jwt)
        {
            // Add null check or try-catch for robustness
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);
            return token.Claims;
        }
    }
}
