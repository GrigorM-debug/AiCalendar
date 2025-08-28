namespace AiCalendar.Blazor.ViewModels.Users
{
    public class LoginResponseViewModel
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }
}
