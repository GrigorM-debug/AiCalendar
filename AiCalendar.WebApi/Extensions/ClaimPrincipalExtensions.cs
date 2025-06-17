using System.Security.Claims;

namespace AiCalendar.WebApi.Extensions
{
    public static class ClaimPrincipalExtensions
    {
        public static string? GetUserId(this ClaimsPrincipal user)
        {
            string? userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
           
            return userId;
        }

        public static string? GetUserName(this ClaimsPrincipal user)
        {
            string? userName = user.FindFirstValue("username");

            return userName;
        }

        public static string? GetEmail(this ClaimsPrincipal user)
        {
            string? email = user.FindFirstValue(ClaimTypes.Email);
            return email;
        }
    }
}
