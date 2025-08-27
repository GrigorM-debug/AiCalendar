using System.IdentityModel.Tokens.Jwt;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Microsoft.JSInterop;

namespace AiCalendar.Blazor.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorageService;
        private readonly string Tokenkey;

        public CustomAuthenticationStateProvider(ILocalStorageService localStorageService)
        {
            _localStorageService = localStorageService;
            Tokenkey = "authToken";
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var token = await _localStorageService.GetItemAsStringAsync(Tokenkey);

                if(string.IsNullOrWhiteSpace(token))
                {
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                var claims = ParseClaimsFromJwt(token);

                var identity = new ClaimsIdentity(claims, "jwt");

                var user = new ClaimsPrincipal(identity);

                return new AuthenticationState(user);
            }
            catch (JSException ex)
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
            catch (Exception)
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        public void NotifyUserAuthentication(string token)
        {
            var claims = ParseClaimsFromJwt(token);
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));

            NotifyAuthenticationStateChanged(authState);
        }

        public void NotifyUserLogout()
        {
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymousUser));

            NotifyAuthenticationStateChanged(authState);
        }

        public async Task<string?> GetToken()
        {
            string? token = await _localStorageService.GetItemAsStringAsync(Tokenkey);

            return token;
        }

        public async Task ClearToken()
        {
            await _localStorageService.RemoveItemAsync(Tokenkey);
        }

        public async Task SetToken(string token)
        {
            await _localStorageService.SetItemAsStringAsync(Tokenkey, token);
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string token)
        {
            var handler = new JwtSecurityTokenHandler();

            var jwtToken = handler.ReadJwtToken(token);

            return jwtToken.Claims;
        }
    }
}
