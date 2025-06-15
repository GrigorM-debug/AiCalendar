using AiCalendar.WebApi.Services.Users.Interfaces;

namespace AiCalendar.WebApi.Services.Users
{
    public class PasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 13);

            return hashedPassword;
        }

        public bool VerifyPassword(string hashedPassword, string password)
        {
            bool isValid = BCrypt.Net.BCrypt.Verify(password, hashedPassword);

            return isValid;
        }
    }
}
