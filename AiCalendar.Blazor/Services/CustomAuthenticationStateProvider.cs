using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace AiCalendar.Blazor.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorageService;
        private readonly string Tokenkey;
        private readonly HttpClient _httpClient;

        public CustomAuthenticationStateProvider(ILocalStorageService localStorageService, HttpClient httpClient)
        {
            _localStorageService = localStorageService;
            Tokenkey = "authToken";
            _httpClient = httpClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _localStorageService.GetItemAsync<string>(Tokenkey);

            var anonymousState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

            if (string.IsNullOrWhiteSpace(token))
            {
                return anonymousState;
            }

            var identity = new ClaimsIdentity();

            var claims = ParseClaimsFromJwt(token);

            var expiry = claims.Where(claim => claim.Type.Equals("exp")).FirstOrDefault();

            if (expiry == null)
                return anonymousState;

            var datetime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiry.Value));

            if (datetime.UtcDateTime <= DateTime.UtcNow)
                return anonymousState;

            identity = new ClaimsIdentity(claims, "jwt");

            var user = new ClaimsPrincipal(identity);

            _httpClient.DefaultRequestHeaders.Authorization = null;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return new AuthenticationState(user);
        }

        public async Task MarkUserAsAuthenticated(string token)
        {
            await _localStorageService.SetItemAsStringAsync(Tokenkey, token);

            _httpClient.DefaultRequestHeaders.Authorization = null;
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var claims = ParseClaimsFromJwt(token);
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));

            NotifyAuthenticationStateChanged(authState);
        }

        public async Task MarkUserAsLoggedOut()
        {
             await _localStorageService.RemoveItemAsync(Tokenkey);

            _httpClient.DefaultRequestHeaders.Authorization = null;

            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymousUser));

            NotifyAuthenticationStateChanged(authState);
        }

        public async Task<string?> GetToken()
        {
            string? token = await _localStorageService.GetItemAsStringAsync(Tokenkey);

            return token;
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string token)
        {
            var handler = new JwtSecurityTokenHandler();

            var jwtToken = handler.ReadJwtToken(token);

            return jwtToken.Claims;
        }
    }
}
