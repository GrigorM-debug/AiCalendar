namespace AiCalendar.WebApi.Services.Users.Interfaces
{
    public interface IPasswordHasher
    {
        public string HashPassword(string password);

        public bool VerifyPassword(string hashedPassword, string password);
    }
}
