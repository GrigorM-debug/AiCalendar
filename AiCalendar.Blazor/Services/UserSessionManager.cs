using Microsoft.AspNetCore.Authentication.Cookies;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace AiCalendar.Blazor.Services
{
    public class UserSessionManager
    { 
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;

        public UserSessionManager(IHttpContextAccessor httpContextAccessor, HttpClient httpClient)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClient;
        }

        public async Task SignInUserAsync(string jwtToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwtToken);
            var principal = new ClaimsPrincipal(new ClaimsIdentity(token.Claims, "jwt"));

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // Remember the user across browser sessions
                ExpiresUtc = token.ValidTo
            };


            if (_httpContextAccessor.HttpContext != null)
            {
                // Store the JWT in the server-side session
                _httpContextAccessor.HttpContext.Session.SetString("JWToken", jwtToken);

                // Sign in the user with the cookie authentication scheme
                await _httpContextAccessor.HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    authProperties);

                //Add the token to http request header
                _httpClient.DefaultRequestHeaders.Authorization = null;
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            }
        }

        public async Task SignOutUserAsync()
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                _httpContextAccessor.HttpContext.Session.Remove("JWToken");
                await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                //Remove jwt token from request header
                _httpClient.DefaultRequestHeaders.Authorization = null;

            }
        }
    }
}
