using AiCalendar.WebApi.Models;

namespace AiCalendar.WebApi.Services.Users.Interfaces
{
    public interface ITokenProvider
    {
        public string GenerateToken(User user);
    }
}
