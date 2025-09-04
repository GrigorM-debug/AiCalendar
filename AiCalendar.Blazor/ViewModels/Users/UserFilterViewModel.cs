namespace AiCalendar.Blazor.ViewModels.Users
{
    public class UserFilterViewModel
    {
        public string? Username { get; set; }

        public string? Email { get; set; }

        public bool? HasActiveEvents { get; set; }
    }
}
