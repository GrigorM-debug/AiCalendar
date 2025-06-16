namespace AiCalendar.WebApi.DTOs.Users
{
    public class UserFilterCriteriaDto
    {
        public string? Username { get; set; }

        public string? Email { get; set; }

        public bool? HasActiveEvents { get; set; }
    }
}
